using System.Threading.RateLimiting;
using DI.Vms.Blazor.Api;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

/*  The API the Android reception app talks to, hosted on its own.

    Why it exists separately from DI.Vms.Blazor, which already serves these same
    endpoints: that app is on UATWEB01 at 192.168.28.13, behind internal DNS. A tablet
    can only reach it from the office LAN. This host is for the case where the tablet is
    not on that LAN.

    It is the same code. Api/VisitsApi.cs, the parser, the reader and the data model are
    compiled into both assemblies from one set of files - see the csproj. A fix to how a
    visit is validated lands in both at once, which is the only arrangement worth having
    when two hosts answer the same requests.

    What is NOT here: the Blazor UI, the in-process card reader, and the toolkit. A card
    is read by the client and arrives as signed XML; this process only verifies and
    stores it. That is what makes a plain net8.0 Linux Web App enough. */

var builder = WebApplication.CreateBuilder(args);

/* Named once. A rate-limiting policy that is not registered throws at startup, so the
   name applied to the endpoints and the name registered below must be the same string. */
const string TabletRateLimit = "tablet";

// ---------------------------------------------------------------- public-host hardening

/* A card read with a photograph is a few hundred kilobytes; the parser refuses anything
   over 8 MB. Kestrel's own default is 30 MB, so without this a caller can make the server
   buffer 30 MB before any of our code looks at it. 12 MB leaves room for the largest
   plausible read and nothing else. */
builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = 12 * 1024 * 1024);

/* App Service terminates TLS at its front end and forwards the request internally, so
   without this every request appears to come from the load balancer. That would make the
   rate limiter one global bucket and every rejection log name the same address - both
   features quietly useless. KnownNetworks and KnownProxies are cleared because the front
   end's addresses are not fixed and not knowable here; this is the documented App Service
   arrangement. */
builder.Services.Configure<ForwardedHeadersOptions>(forwarded =>
{
    forwarded.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    forwarded.KnownNetworks.Clear();
    forwarded.KnownProxies.Clear();
});

/* Turns an unhandled exception into a ProblemDetails response. Without it the client gets
   an empty 500 and the reason lives only in the log; with it the client gets a structured
   answer and still no stack trace, because this is not the developer exception page. */
builder.Services.AddProblemDetails();

/* Rate limiting, per calling address per minute.

   The endpoint most worth limiting is /api/people: with a valid credential it answers
   name-fragment queries out of a 725-person staff directory, twelve at a time. A limit
   does not stop enumeration by someone holding the key - only Entra and roles do that -
   but it turns "scrape the directory in a minute" into something slow and visible in the
   logs.

   300 a minute is generous for reception: a check-in is roughly twenty requests including
   the debounced host search, and several tablets behind one office NAT share an address.
   It is a flood limit, not a quota. */
builder.Services.AddRateLimiter(limiter =>
{
    limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    limiter.AddPolicy<string>(TabletRateLimit, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    limiter.OnRejected = (context, _) =>
    {
        context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiter")
            .LogWarning(
                "Rate limit reached by {Address} on {Path}.",
                context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "(unknown)",
                context.HttpContext.Request.Path);

        return ValueTask.CompletedTask;
    };
});

// ---------------------------------------------------------------- who may call

var signIn = SignInOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(signIn);

/* Throws at startup when there is neither Entra nor a key. The one thing this host must
   never be is reachable and unguarded. */
var apiKey = ApiKey.RequireForPublicHost(builder.Configuration, signIn.Enabled);

if (signIn.Enabled)
{
    /* Bearer tokens only. There is no browser here and no cookie: the Blazor app owns
       the interactive sign-in, this owns the tablet. */
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(
            jwtOptions =>
            {
                jwtOptions.TokenValidationParameters.RoleClaimType = "roles";
                jwtOptions.TokenValidationParameters.NameClaimType = "name";
            },
            identityOptions => builder.Configuration.GetSection("AzureAd").Bind(identityOptions),
            jwtBearerScheme: JwtBearerDefaults.AuthenticationScheme);
}
else
{
    /* The same open-desk scheme the Blazor app uses when sign-in is off: it always
       succeeds and carries every role, so the policies below stay written as they are and
       go on being evaluated. Here the API key is what actually guards the door, and
       RecordedBy reads "(not signed in)" on every entry made this way. */
    builder.Services
        .AddAuthentication(OpenDeskAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, OpenDeskAuthenticationHandler>(
            OpenDeskAuthenticationHandler.SchemeName, _ => { });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(VmsRoles.CanCheckIn, policy => policy.RequireRole(
        VmsRoles.Officer, VmsRoles.Supervisor, VmsRoles.Admin, VmsRoles.SystemAdmin));

    options.AddPolicy(VmsRoles.CanViewReport, policy => policy.RequireRole(
        VmsRoles.Supervisor, VmsRoles.Admin, VmsRoles.SystemAdmin));

    options.AddPolicy(VmsRoles.CanViewUnmaskedId, policy => policy.RequireRole(VmsRoles.UnmaskedId));

    // Anything added later is protected unless it says otherwise. /health says otherwise.
    options.FallbackPolicy = options.DefaultPolicy;
});

// ---------------------------------------------------------------- data

/* EnableRetryOnFailure, unlike the on-premises host. Azure SQL moves databases between
   nodes and throttles, and both surface as transient failures on a connection that was
   fine a second earlier. Without this a routine failover shows up at the desk as a failed
   check-in. */
builder.Services.AddDbContextFactory<VmsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Vms"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

// ---------------------------------------------------------------- the card path

/* Agent mode, always. This process has no reader and never will - the InProcess reader is
   not even compiled in. The options still carry the signature rules, which are what
   matter here: RequireSignature, the trusted signer thumbprints, the maximum age. */
/* The host list. Same resolution as the Blazor host, so a tablet talking to this API and a
   browser talking to that one offer the same people. */
var directory = DirectoryOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(directory);

if (directory.UsesEntraId)
{
    builder.Services.AddHttpClient(EntraStaffDirectory.HttpClientName);
    builder.Services.AddSingleton<IStaffDirectory, EntraStaffDirectory>();
}
else
{
    builder.Services.AddSingleton<IStaffDirectory, NoStaffDirectory>();
}

var capture = CardCaptureOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(capture);
builder.Services.AddSingleton(capture.Agent);

/* Singleton because it holds the outstanding request IDs a read is redeemed against.
   One instance, one set of IDs, each spent once.

   Worth knowing before this host is scaled out: that state is in memory. On two or more
   instances a read begun on one and completed on another is rejected, because the second
   instance never issued the ID. Keep the Web App at one instance, or move the outstanding
   IDs to a shared store first. */
builder.Services.AddSingleton<AgentCardReader>();

var app = builder.Build();

// ---------------------------------------------------------------- startup checks

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<VmsDbContext>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await using var db = await factory.CreateDbContextAsync();

    /* Creates absent tables and refuses to start if the model has columns the database
       does not, naming them. The same check the Blazor app runs, and worth repeating
       here: this host may well meet a database that the other one migrated. */
    await DbBootstrapper.EnsureSchemaAsync(db, logger);

    capture.LogTo(logger);
    signIn.LogTo(logger);

    if (!signIn.Enabled)
    {
        logger.LogWarning(
            "This API is guarded by an API key only. Every visit it records is attributed to " +
            "\"{RecordedBy}\" rather than a person. Turn Authentication:Enabled on once the Entra " +
            "registration is finished.",
            SignInOptions.NotSignedIn);
    }
}

// ---------------------------------------------------------------- pipeline

/* No UseHttpsRedirection. Azure App Service terminates TLS at the front end and forwards
   plain HTTP inside, so redirecting here either does nothing or loops. The right control
   is "HTTPS Only" on the Web App, which refuses the plain request before it arrives. */

/* First, so everything downstream - the rate limiter's partition, the rejection log, the
   audit trail - sees the caller's real address rather than the load balancer's. */
app.UseForwardedHeaders();

app.UseExceptionHandler();

app.UseAuthentication();

// Before authorisation: a request without the key is refused without reaching a policy.
app.UseApiKey(signIn.Enabled ? null : apiKey);

app.UseRateLimiter();

app.UseAuthorization();

app.MapVisitsApi(signIn.Enabled, TabletRateLimit);

/* For App Service's health check, and for answering "is it the API or the network?"
   without a card or a tablet. Anonymous on purpose - a probe that needs a credential is a
   probe that reports the credential's health, not the app's - and it deliberately says
   nothing about why the database is unreachable, since that would describe the inside of
   the network to anyone who asks. */
app.MapGet("/health", async (IDbContextFactory<VmsDbContext> factory, CancellationToken ct) =>
{
    var database = await CanReachDatabase(factory, ct);

    /* One word, and no more. App Service's health probe needs to know whether to take the
       instance out of rotation; an anonymous caller on the internet needs to know nothing
       else. "Which authentication is configured" and "is the database reachable" are both
       useful to somebody deciding whether to keep prodding. */
    return Results.Ok(new { status = database ? "ok" : "degraded" });
}).AllowAnonymous();

/* The same question with the detail, behind the same credential as everything else under
   /api - so it is answerable when something is wrong, without publishing the shape of the
   deployment to anyone who asks. */
app.MapGet("/api/health", async (IDbContextFactory<VmsDbContext> factory, CancellationToken ct) =>
{
    var database = await CanReachDatabase(factory, ct);

    return Results.Ok(new
    {
        status = database ? "ok" : "degraded",
        database = database ? "ok" : "unreachable",
        authentication = signIn.Enabled ? "entra" : "api-key",
        signatureRequired = capture.Agent.RequireSignature,
    });
}).RequireAuthorization(VmsRoles.CanCheckIn).RequireRateLimiting(TabletRateLimit);

app.Run();

static async Task<bool> CanReachDatabase(IDbContextFactory<VmsDbContext> factory, CancellationToken ct)
{
    try
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Database.CanConnectAsync(ct);
    }
    catch (Exception)
    {
        return false;
    }
}

