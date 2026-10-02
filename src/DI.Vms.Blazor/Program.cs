using DI.Vms.Blazor.Components;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
using DI.Vms.Blazor.Api;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

/* The rate-limit policy's name, used where it is defined and where the API group
   requires it. */
const string ApiRateLimit = "api";

var builder = WebApplication.CreateBuilder(args);

/* A local overlay for the machine this is running on, and the answer to "where do I put the
   connection string?" on a development box.

   appsettings.json holds no credentials - it is committed, and a secret committed once is
   in the history for good. appsettings.Development.json is committed too. This file is not:
   .gitignore has covered appsettings.*.Local.json since the beginning, and nothing loaded
   one, so there was no place for a developer's own connection string to go.

   Development only, deliberately. In Azure the settings come from the Web App's
   configuration, which arrives as environment variables; a JSON file added here would sit
   above those in the order and silently win if one were ever deployed by accident. Scoping
   it to Development means it cannot. */
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.Local.json", optional: true, reloadOnChange: true);
}

/* So the reception PC can serve the desk from boot with nobody logged in. This checks
   whether the process really was started by the service control manager and does nothing
   when it was not, so `dotnet run` is unaffected. It also sets the content root to the
   executable's folder - a service starts in C:\Windows\System32 otherwise, and would
   find neither wwwroot nor appsettings.json. See docs/deployment.md.

   Out of the portable build along with its package: a Windows service is not a thing on
   the host that build exists for. App Service keeps the process alive there instead. */
#if !VMS_AGENT_ONLY
builder.Services.AddWindowsService(options => options.ServiceName = "DI VMS");
#endif

/* App Service terminates TLS at its front end and forwards the request to the container
   over plain HTTP, so without this the app believes every request is http - and that is
   what breaks sign-in.

   The OpenID Connect handler writes its correlation and nonce cookies as SameSite=None,
   because the callback is a cross-site form POST from login.microsoftonline.com and
   nothing else survives that. It marks them Secure only when it thinks the request is
   HTTPS. A SameSite=None cookie without Secure is rejected outright by Chrome, so the
   correlation cookie is never stored, the callback arrives without it, the handler fails
   correlation and challenges again - which at the desk looks like a password prompt that
   will not go away.

   KnownNetworks and KnownProxies are cleared because App Service's front-end addresses
   are not fixed and not knowable here; this is the documented arrangement. */
builder.Services.Configure<ForwardedHeadersOptions>(forwarded =>
{
    forwarded.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    forwarded.KnownNetworks.Clear();
    forwarded.KnownProxies.Clear();
});

/* Sign-in: on with Entra ID, or off with the desk trusted as it was before any of this
   existed. One switch, resolved here, and everything downstream reads it from the
   container rather than from configuration again. See Services/SignInOptions.cs. */
var signIn = SignInOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(signIn);

/* An API key for /api, if one is configured. Optional here and required on the Azure-only
   host, and the difference is deliberate: UATWEB01 sits inside the office network and has
   never had one, so demanding it would break a working deployment to protect it from a
   threat it does not have. Set Api:Key whenever this app is reachable from the internet -
   and note that a key protects /api only. The Blazor screens, the visitor report
   included, are protected by sign-in or by nothing. */
var apiKey = ApiKey.Configured(builder.Configuration);
var tabletKeys = ApiKey.AllConfigured(builder.Configuration);

if (signIn.Enabled)
{
    /* Entra ID over OpenID Connect.

       Chosen over Windows Authentication because it survives a tablet browser and a
       connection from outside the office, and brings MFA and conditional access with it -
       none of which Negotiate does. Sign-in is silent on a domain-joined desk that Entra
       already knows, so reception sees no prompt; see docs/entra-id-setup.md.

       IIS must be serving this site with Anonymous authentication ON and Windows
       Authentication OFF, or IIS challenges the browser before the request ever reaches
       the OpenID Connect handler. install-iis.ps1 sets it that way. */
    /* Code flow redeems the code at the token endpoint with the client secret, so a
       half-filled AzureAd section is a sign-in that fails at the redirect rather than at
       startup - with an error page in front of reception and nothing in it naming the
       missing setting. Refuse here instead, where the message can say which one. */
    foreach (var (name, value) in new[]
             {
                 ("AzureAd:TenantId", builder.Configuration["AzureAd:TenantId"]),
                 ("AzureAd:ClientId", builder.Configuration["AzureAd:ClientId"]),
                 ("AzureAd:ClientSecret", builder.Configuration["AzureAd:ClientSecret"]),
             })
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Authentication:Enabled is true but {name} is not set. Sign-in cannot work without " +
                "it. Set it in the Web App's configuration (double underscores: " +
                $"{name.Replace(":", "__")}) or in appsettings.Production.json, or set " +
                "Authentication:Enabled to false. See docs/entra-id-setup.md.");
        }
    }

    var authentication = builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme);

    authentication.AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.GetSection("AzureAd").Bind(options);

        /* Authorization code flow, stated rather than inherited.

           ASP.NET Core's OpenIdConnectOptions defaults ResponseType to id_token - the
           implicit flow - and Entra refuses that unless the registration has "ID tokens"
           ticked under Implicit grant and hybrid flows. The symptom is
           AADSTS700054 on the first sign-in, which names a response type nobody chose.

           Ticking the box in the portal would also clear the error, and would be the wrong
           fix: the implicit flow returns the token in the browser's URL fragment, it is
           deprecated by the OAuth working group, and it exists for JavaScript apps that
           cannot keep a secret. This one can - AzureAd:ClientSecret is what it uses to
           redeem the code at the token endpoint - so it should use the flow that keeps the
           token out of the address bar. PKCE is on by default and rides along with it. */
        options.ResponseType = OpenIdConnectResponseType.Code;

        /* Claims keep the names Entra gave them.

           This line is the whole of a bug that cost a day. The handler's inbound claim map
           is on by default and renames "roles" to
           http://schemas.microsoft.com/ws/2008/06/identity/claims/role - so setting
           RoleClaimType to "roles" below made every role check look for a claim type that
           the mapping had just removed. Entra sent Vms.SystemAdmin, the token carried it,
           and IsInRole found nothing: every policy failed for everybody, and it read
           exactly like an app-role assignment that had not been made.

           Off, "roles", "preferred_username" and "oid" arrive under those names - the ones
           Microsoft's own documentation uses - and the two lines below mean what they say.
           VisitsApi and NewVisitor already read preferred_username directly, so this also
           makes the audit column agree with the token rather than with a WS-Federation
           alias of it. */
        options.MapInboundClaims = false;

        /* Entra sends app roles in "roles", and the name of the signed-in person in
           "preferred_username" - Microsoft.Identity.Web overrides NameClaimType to that
           anyway, so it is stated here rather than left to look like an accident. */
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.TokenValidationParameters.NameClaimType = "preferred_username";
    });

    /* And bearer tokens beside the cookie, for the Android reception app. Two schemes on
       purpose: the browser carries a session cookie, the tablet carries an access token
       for this API's own scope, and neither is accepted where the other belongs - so a
       cookie lifted from a desk browser cannot be replayed against the API. */
    authentication.AddMicrosoftIdentityWebApi(
        jwtOptions =>
        {
            // The same mapping, and the same trap. See the note on the web app above.
            jwtOptions.MapInboundClaims = false;
            jwtOptions.TokenValidationParameters.RoleClaimType = "roles";
            jwtOptions.TokenValidationParameters.NameClaimType = "preferred_username";
        },
        identityOptions => builder.Configuration.GetSection("AzureAd").Bind(identityOptions),
        jwtBearerScheme: JwtBearerDefaults.AuthenticationScheme);

    /* Where a signed-in person with no role is sent.

       The default is /MicrosoftIdentity/Account/AccessDenied, and Microsoft.Identity.Web.UI
       does not implement that action - so the first failure this deployment was always
       going to hit, a user signed in with no app role, arrived as a bare 404 with nothing
       in it naming a role. Components/Pages/AccessDenied.razor says what is missing and who
       assigns it. */
    builder.Services.Configure<CookieAuthenticationOptions>(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options => options.AccessDeniedPath = "/access-denied");

    /* And the tablet's key beside both.

       The reception tablet does not sign in and is not going to: it sits on a counter, it
       is handed to nobody, and a device in the room the visitors are in cannot hold a
       person's credential. So it presents a shared key and arrives as the desk - one role,
       CanCheckIn, and RecordedBy says "(not signed in)" because that is what happened.

       Without this, turning sign-in on for the web app answers 401 to every screen on every
       tablet, which is a working reception desk broken by a setting that was about the
       browser. */
    if (tabletKeys.Count > 0)
    {
        authentication.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationHandler.SchemeName,
            options => options.Keys = tabletKeys);
    }
}
else
{
    /* One scheme that always succeeds, carrying every role. The policies below and the
       [Authorize] attributes on the pages are left exactly as they are and go on being
       evaluated - they just all pass. Rules that are switched off rather than satisfied
       are rules nobody finds the bugs in until the day they matter. */
    builder.Services
        .AddAuthentication(OpenDeskAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, OpenDeskAuthenticationHandler>(
            OpenDeskAuthenticationHandler.SchemeName, _ => { });
}

/*  The save endpoint's input validation, which DI-IT-POL-AIDEV-001 §4.4 requires to be
    expressed with FluentValidation.

    Registered by hand rather than with AddValidatorsFromAssembly, so that the two
    validators this application has are named here and a third cannot appear by being
    dropped into a folder. Singleton because both are stateless - they hold rules, not
    state - and the rules themselves still live in Services/VisitorFields.cs, which the
    screens and the tablet are written against. */
builder.Services.AddSingleton<IValidator<SaveVisitRequest>, SaveVisitRequestValidator>();
builder.Services.AddSingleton<IValidator<CardData>, CardIdentityValidator>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(VmsRoles.CanCheckIn, policy => policy.RequireRole(
        VmsRoles.Officer, VmsRoles.Supervisor, VmsRoles.Admin, VmsRoles.SystemAdmin));

    options.AddPolicy(VmsRoles.CanViewReport, policy => policy.RequireRole(
        VmsRoles.Supervisor, VmsRoles.Admin, VmsRoles.SystemAdmin));

    options.AddPolicy(VmsRoles.CanViewUnmaskedId, policy => policy.RequireRole(VmsRoles.UnmaskedId));

    /* Nothing is anonymous. A page added later is protected by default rather than by
       whoever remembers the attribute - the wrong way round for a visitor log. */
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddCascadingAuthenticationState();

var controllers = builder.Services.AddControllersWithViews();

if (signIn.Enabled)
{
    // Supplies /MicrosoftIdentity/Account/SignIn and SignOut.
    controllers.AddMicrosoftIdentityUI();
}

builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options =>
{
    /* A photograph crosses the circuit, and the default ceiling is 32 KB.
       
       That default is right for a Blazor app whose interop carries form values. This one
       carries a picture of an Emirates ID from the browser's camera to the server, which is
       a few hundred kilobytes of base64 - and over the limit the call is rejected with no
       error anyone sees, so the camera shows a clear picture and the capture button appears
       to do nothing. That is exactly what it did.

       Eight megabytes is a ceiling, not a reservation: nothing is allocated until a message
       arrives, and DigitalCard:MaximumBytes refuses anything larger before it is sent. */
    options.MaximumReceiveMessageSize = 8 * 1024 * 1024;
});

/* EnableRetryOnFailure, because this app now runs against Azure SQL as well as against
   SQL Server on UATWEB01. Azure SQL moves a database between nodes and throttles, and both
   surface as a failure on a connection that was fine a second earlier; without this a
   routine failover shows up at the desk as a failed check-in. It costs nothing on-premises,
   where those failures do not happen. */

/* Checked here rather than left to the first query. Without it the failure is
   "The ConnectionString property has not been initialized" thrown from inside EF at
   startup, which says nothing about which setting is missing or where it should go. */
var connectionString = builder.Configuration.GetConnectionString("Vms");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No connection string named Vms. In Azure, set it in the Web App's configuration - " +
        "either as a connection string of type SQLAzure named Vms, or as the application " +
        "setting ConnectionStrings__Vms. On UATWEB01 it goes in appsettings.Production.json. " +
        "On a development machine, in Visual Studio: right-click the project in Solution " +
        "Explorer, choose Manage User Secrets, and paste\n\n" +
        "  { \"ConnectionStrings\": { \"Vms\": \"Server=...;Database=vms;...\" } }\n\n" +
        "That file lives outside the repository, so it cannot be committed. Without Visual " +
        "Studio, run deploy\\dev-settings.ps1 instead.\n\n" +
        "It is never in appsettings.json, which is committed: a credential pushed once is " +
        "in the repository's history for good.");
}

builder.Services.AddDbContextFactory<VmsDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

/* Where the image of the card that was read is kept: Azure Blob Storage when a storage
   account is configured, the database column when it is not. UATWEB01 has no storage
   account and needs none - see Services/CardImageStore.cs for why blob is the better home
   for it in Azure. */
builder.Services.AddSingleton(CardImageStorageOptions.FromConfiguration(builder.Configuration));
builder.Services.AddSingleton<CardImageStore>();

/* Reading the machine-readable zone off a photograph of a card, for a visitor who has the
   card on a phone rather than in a wallet. The recognition runs in the browser and costs
   nothing; the checking runs here. Off unless a deployment turns it on. */
var digitalCard = DigitalCardOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(digitalCard);

/* Where the host list comes from: the Entra ID tenant people already sign in with, or the
   vms.Person table the AD export was loaded into. Resolved once, so the desk screen and
   the tablet's /api/people cannot disagree about it. */
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

/* Where the reader is, relative to this process - the deployment's central decision.
   Resolved once here so both readers and every screen agree on it. */
/*  Rate limiting, required of every API endpoint by DI-IT-POL-AIDEV-001 §8.5.
 *
 *  The standalone API project has had this since it was written; this host - the one that is
 *  actually deployed - did not, so the setting existed and was switched off by being null.
 *
 *  Partitioned by caller address rather than globally: two reception desks and two tablets
 *  share this host, and a global bucket would let a stuck tablet retrying in a loop lock the
 *  desks out. The limit is generous because the camera scanner posts a frame at a time while
 *  a card is held up - it is there to stop a runaway client and a scripted probe, not to
 *  pace ordinary use.
 */
builder.Services.AddRateLimiter(limiter =>
{
    limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    limiter.AddPolicy<string>(ApiRateLimit, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    /* Logged, because a limit that is being hit silently is indistinguishable from an
       application that is simply slow. Address and path only - never the key. */
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

/*  The session cookie, hardened as §8.1 requires.
 *
 *  Only the application cookie. The handshake cookies Microsoft.Identity.Web uses during
 *  sign-in - the nonce and the correlation - are deliberately left as the library sets them:
 *  Entra posts the response back cross-site, and a Strict cookie is not sent on that request,
 *  so forcing Strict there does not harden sign-in, it breaks it.
 *
 *  Always secure rather than SameAsRequest, because the deployment that matters is the
 *  internet-facing one; the reception PC binds 127.0.0.1 where there is no certificate and
 *  no sign-in either, so nothing there depends on this cookie.
 */
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

var capture = CardCaptureOptions.FromConfiguration(builder.Configuration);

builder.Services.AddSingleton(capture);
builder.Services.AddSingleton(capture.Agent);

/* The in-process reader, where this build has one.

   Singleton: the toolkit is a native context that is expensive to create and must not be
   initialised concurrently, and the service serialises access internally.

   VMS_AGENT_ONLY is the portable build - net8.0, no ICP toolkit, runs on Linux. There
   CardReaderService is not compiled at all, and what is registered says there is no reader
   here, which is an answer the screens already know how to show. See the VmsAgentOnly
   property in DI.Vms.Blazor.csproj. */
#if VMS_AGENT_ONLY
builder.Services.AddSingleton<ICardReader, NoCardReader>();
#else
builder.Services.AddSingleton<ICardReader, CardReaderService>();
#endif

/* Singleton because it holds the outstanding request IDs a browser's read is redeemed
   against. Scoped per circuit would let a second tab replay the first tab's read. */
builder.Services.AddSingleton<AgentCardReader>();

var app = builder.Build();

/* Creates the tables if they are absent, so neither a fresh database nor UATWEB01 -
   which already exists, holding tables from an earlier design - needs a separate step.
   Schema only: the entity list is data, maintained in the database by script. */
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<VmsDbContext>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await DbBootstrapper.EnsureSchemaWithRetryAsync(factory, logger).ConfigureAwait(false);

    capture.LogTo(logger);
    signIn.LogTo(logger);

    logger.LogInformation(
        digitalCard.Enabled
            ? "Digital card scanning is ON. The recogniser is served from {Engine}."
            : "Digital card scanning is off. Set DigitalCard:Enabled to true to offer it.",
        digitalCard.EngineBaseUrl);

    /* The sign-in flow, by name, at every startup.

       Twice now a sign-in failure has been diagnosed from the outside as configuration
       when the real answer was that the running build predated the fix - and nothing in
       the log said which build was running. This resolves the options the framework
       actually ends up with, after every library has had its say, so the answer to "is
       the code-flow build deployed?" is one line rather than an inference from an
       absence of AADSTS700054. */
    if (signIn.Enabled)
    {
        var oidc = scope.ServiceProvider
            .GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>()
            .Get(OpenIdConnectDefaults.AuthenticationScheme);

        logger.LogInformation(
            "Sign-in uses the {ResponseType} flow, returning to {CallbackPath}.",
            oidc.ResponseType,
            oidc.CallbackPath);
    }

    logger.Log(
        signIn.Enabled || tabletKeys.Count > 0 ? LogLevel.Information : LogLevel.Warning,
        "The /api endpoints are guarded by {Guard}.",
        (signIn.Enabled, tabletKeys.Count > 0) switch
        {
            (true, true) => "an Entra ID token or the tablet's API key",
            (true, false) => "an Entra ID token - no Api:Key is set, so no tablet can reach them",
            (false, true) => "an API key",
            (false, false) => "nothing but the network this server is on",
        });

    /* Which tablets can reach it, by name. A desk that says "the report does not show which
       tablet recorded this" is usually a tablet holding the unnamed Api:Key, and this line is
       where that shows. Names only - a key is never logged, not even a prefix. */
    if (tabletKeys.Count > 0)
    {
        logger.LogInformation(
            "Tablet keys configured: {Tablets}.",
            string.Join(", ", tabletKeys.Select(k => k.Name)));
    }

    BrandAssets.Locate(app.Environment.WebRootPath, logger);
}

/* First in the pipeline, so everything after it - the exception handler's links, the
   HTTPS redirect, and above all the authentication cookies - sees the scheme the browser
   actually used rather than the one the container was handed. */
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
}

/* Only when an HTTPS endpoint actually exists. The reception-PC deployment binds
   http://127.0.0.1 and nothing else - there is no certificate on that machine and no
   network listener to protect - and redirecting to a port nothing is listening on would
   take the desk offline. HSTS goes with it: sent over plain HTTP it is ignored, and sent
   from a host that later drops HTTPS it locks the browser out. */
if (HasHttpsEndpoint(builder.Configuration))
{
    if (!app.Environment.IsDevelopment()) { app.UseHsts(); }
    app.UseHttpsRedirection();
}

/* Before the static files and the endpoints, so every response carries them - a script or a
   stylesheet is as much a thing a browser enforces a policy on as a page. */
app.UseVmsSecurityHeaders(digitalCard, capture, HasHttpsEndpoint(builder.Configuration));

app.UseStaticFiles();

/* Order matters: authentication establishes who, authorisation decides what, and
   antiforgery must sit after both so its tokens are bound to an identity. */
app.UseAuthentication();

// Before authorisation, so a request without the key never reaches a policy.
/*  Every configured key, not just the unnamed one.
 *
 *  This took the single Api:Key and nothing else, which was correct until named keys existed
 *  and a hole the moment they did: a host with sign-in off and only Api:Keys:<name> set got
 *  null here, installed no guard at all, and then authenticated every caller through the
 *  open-desk scheme. The API would have been open to anyone who could reach it, with the
 *  configuration looking entirely deliberate.
 *
 *  Not the deployment that found it - that one has sign-in on, where the key is an
 *  authentication scheme and this middleware does not run. It is the on-premises host that
 *  would have been exposed. */
app.UseApiKey(signIn.Enabled ? [] : tabletKeys);

app.UseAuthorization();
app.UseAntiforgery();

// After authorisation, so a rejected request is not counted against a caller's allowance.
app.UseRateLimiter();

app.MapControllers();

/* The Android reception app's endpoints. With sign-in on they take an Entra token or the
   tablet's key; with it off, the open-desk scheme and the key middleware above. */
app.MapVisitsApi(signIn.Enabled, acceptTabletKey: signIn.Enabled && tabletKeys.Count > 0, rateLimitPolicy: ApiRateLimit);

/* The stored card for one visit.

   Behind the report's own policy, because it is the same data the report shows and the
   same people should see it.

   Served inline so a click opens it, with headers that make that safe: an SVG is a
   document a browser will execute script in, and this one is served from the app's own
   origin. Nothing in a generated card carries script - every value goes through XML
   escaping - but "nothing does today" is not a control. The policy says: no scripts, no
   network, images only from the data URI that is already inside the file. */
app.MapGet("/visits/{id:int}/card", async (
    int id,
    IDbContextFactory<VmsDbContext> factory,
    CardImageStore images,
    HttpContext http,
    CancellationToken ct) =>
{
    await using var db = await factory.CreateDbContextAsync(ct);

    var record = await db.VisitorCardImages
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.VisitorEntryId == id, ct);

    if (record is null) return Results.NotFound();

    /* Read here and served from this endpoint rather than redirected to the blob. A
       redirect - even a signed one - would put the visitor's photograph on a URL that
       leaves this app's authorization behind and can be forwarded; and the file is an SVG,
       which is a document that can carry script, so it has to arrive under the policy
       below rather than on the storage account's origin. */
    var bytes = await images.ReadAsync(record, ct);

    // The row says there is an image and there is not: a deleted blob, not a server fault.
    if (bytes is null) return Results.NotFound();

    http.Response.Headers["Content-Security-Policy"] =
        "default-src 'none'; style-src 'unsafe-inline'; img-src data:; sandbox";
    http.Response.Headers["X-Content-Type-Options"] = "nosniff";

    return Results.File(bytes, record.ContentType);
}).RequireAuthorization(VmsRoles.CanViewReport);

/* App Service wants a health check path, and it is the quickest way to tell "the app is
   down" from "the network is in the way" without opening a browser or holding a card.

   Anonymous - everything else is behind the fallback policy - and it answers 200 either
   way, with the verdict in the body. A 503 would have App Service take the instance out of
   rotation and restart it, which for a single-instance app with a briefly unreachable
   database turns a blip into a restart loop. */
/*  Which build is actually answering.
 *
 *  The sidebar already carries the stamp, but it takes a sign-in to see and a screen to read.
 *  This takes a browser address bar, says the same thing, and settles "nothing changed" in one
 *  look - which has cost this project more rounds than any bug in it.
 */
app.MapGet("/version", () => Results.Text(
    $"{BuildInfo.Stamp}\ncache tag {BuildInfo.CacheTag}\n", "text/plain"))
   .AllowAnonymous();

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

    return Results.Ok(new { status = database ? "ok" : "degraded" });
}).AllowAnonymous();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

/* Both the places a URL can come from: --urls / ASPNETCORE_URLS (both land on the "urls"
   key) and Kestrel:Endpoints in appsettings. */
static bool HasHttpsEndpoint(IConfiguration configuration)
{
    var urls = configuration["urls"];
    if (urls is not null && urls.Contains("https://", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return configuration.GetSection("Kestrel:Endpoints").GetChildren().Any(endpoint =>
        endpoint["Url"]?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == true);
}
