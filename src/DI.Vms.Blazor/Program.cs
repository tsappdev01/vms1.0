using DI.Vms.Blazor.Components;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
using DI.Vms.Blazor.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

/* So the reception PC can serve the desk from boot with nobody logged in. This checks
   whether the process really was started by the service control manager and does nothing
   when it was not, so `dotnet run` is unaffected. It also sets the content root to the
   executable's folder - a service starts in C:\Windows\System32 otherwise, and would
   find neither wwwroot nor appsettings.json. See docs/deployment.md. */
builder.Services.AddWindowsService(options => options.ServiceName = "DI VMS");

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
    var authentication = builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme);

    authentication.AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.GetSection("AzureAd").Bind(options);

        /* Entra sends app roles in "roles". Without this the framework looks for the long
           WS-Federation role claim, finds nothing, and every policy fails for everyone -
           which reads like a directory problem and is not one. */
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.TokenValidationParameters.NameClaimType = "name";
    });

    /* And bearer tokens beside the cookie, for the Android reception app. Two schemes on
       purpose: the browser carries a session cookie, the tablet carries an access token
       for this API's own scope, and neither is accepted where the other belongs - so a
       cookie lifted from a desk browser cannot be replayed against the API. */
    authentication.AddMicrosoftIdentityWebApi(
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
    /* One scheme that always succeeds, carrying every role. The policies below and the
       [Authorize] attributes on the pages are left exactly as they are and go on being
       evaluated - they just all pass. Rules that are switched off rather than satisfied
       are rules nobody finds the bugs in until the day they matter. */
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

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

/* EnableRetryOnFailure, because this app now runs against Azure SQL as well as against
   SQL Server on UATWEB01. Azure SQL moves a database between nodes and throttles, and both
   surface as a failure on a connection that was fine a second earlier; without this a
   routine failover shows up at the desk as a failed check-in. It costs nothing on-premises,
   where those failures do not happen. */
builder.Services.AddDbContextFactory<VmsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Vms"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

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
var capture = CardCaptureOptions.FromConfiguration(builder.Configuration);

builder.Services.AddSingleton(capture);
builder.Services.AddSingleton(capture.Agent);

/* Singleton: the toolkit is a native context that is expensive to create and must not be
   initialised concurrently. The service serialises access internally. */
builder.Services.AddSingleton<CardReaderService>();

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

    await using var db = await factory.CreateDbContextAsync();

    await DbBootstrapper.EnsureSchemaAsync(db, logger);

    capture.LogTo(logger);
    signIn.LogTo(logger);

    if (!signIn.Enabled)
    {
        logger.Log(
            apiKey is null ? LogLevel.Warning : LogLevel.Information,
            "The /api endpoints are guarded by {Guard}.",
            apiKey is null ? "nothing but the network this server is on" : "an API key");
    }

    BrandAssets.Locate(app.Environment.WebRootPath, logger);
}

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

app.UseStaticFiles();

/* Order matters: authentication establishes who, authorisation decides what, and
   antiforgery must sit after both so its tokens are bound to an identity. */
app.UseAuthentication();

// Before authorisation, so a request without the key never reaches a policy.
app.UseApiKey(signIn.Enabled ? null : apiKey);

app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();

// The Android reception app's endpoints. Bearer-only when sign-in is on; see Api/VisitsApi.cs.
app.MapVisitsApi(signIn.Enabled);

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
    HttpContext http,
    CancellationToken ct) =>
{
    await using var db = await factory.CreateDbContextAsync(ct);

    var image = await db.VisitorCardImages
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.VisitorEntryId == id, ct);

    if (image is null) return Results.NotFound();

    http.Response.Headers["Content-Security-Policy"] =
        "default-src 'none'; style-src 'unsafe-inline'; img-src data:; sandbox";
    http.Response.Headers["X-Content-Type-Options"] = "nosniff";

    return Results.File(image.Image, image.ContentType);
}).RequireAuthorization(VmsRoles.CanViewReport);

/* App Service wants a health check path, and it is the quickest way to tell "the app is
   down" from "the network is in the way" without opening a browser or holding a card.

   Anonymous - everything else is behind the fallback policy - and it answers 200 either
   way, with the verdict in the body. A 503 would have App Service take the instance out of
   rotation and restart it, which for a single-instance app with a briefly unreachable
   database turns a blip into a restart loop. */
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
