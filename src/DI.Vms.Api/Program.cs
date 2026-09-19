using DI.Vms.Api;
using DI.Vms.Blazor.Api;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// ---------------------------------------------------------------- who may call

var signIn = SignInOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(signIn);

/* Throws at startup when there is neither Entra nor a key. The one thing this host must
   never be is reachable and unguarded. */
var apiKey = ApiKey.Require(builder.Configuration, signIn.Enabled);

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

app.UseAuthentication();

// Before authorisation: a request without the key is refused without reaching a policy.
app.UseApiKey(signIn.Enabled ? null : apiKey);

app.UseAuthorization();

app.MapVisitsApi(signIn.Enabled);

/* For App Service's health check, and for answering "is it the API or the network?"
   without a card or a tablet. Anonymous on purpose - a probe that needs a credential is a
   probe that reports the credential's health, not the app's - and it deliberately says
   nothing about why the database is unreachable, since that would describe the inside of
   the network to anyone who asks. */
app.MapGet("/health", async (IDbContextFactory<VmsDbContext> factory, CancellationToken ct) =>
{
    bool database;
    try
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        database = await db.Database.CanConnectAsync(ct);
    }
    catch (Exception)
    {
        database = false;
    }

    return Results.Ok(new
    {
        status = database ? "ok" : "degraded",
        database = database ? "ok" : "unreachable",
        authentication = signIn.Enabled ? "entra" : "api-key",
    });
}).AllowAnonymous();

app.Run();
