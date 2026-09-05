using DI.Vms.Blazor.Components;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
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

/* Entra ID over OpenID Connect.
   
   Chosen over Windows Authentication because it survives a tablet browser and a
   connection from outside the office, and brings MFA and conditional access with it -
   none of which Negotiate does. Sign-in is silent on a domain-joined desk that Entra
   already knows, so reception sees no prompt; see docs/entra-id-setup.md.

   IIS must be serving this site with Anonymous authentication ON and Windows
   Authentication OFF, or IIS challenges the browser before the request ever reaches the
   OpenID Connect handler. install-iis.ps1 sets it that way. */
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.GetSection("AzureAd").Bind(options);

        /* Entra sends app roles in "roles". Without this the framework looks for the
           long WS-Federation role claim, finds nothing, and every policy fails for
           everyone - which reads like a directory problem and is not one. */
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.TokenValidationParameters.NameClaimType = "name";
    });

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

// Supplies /MicrosoftIdentity/Account/SignIn and SignOut.
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<VmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Vms")));

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
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
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
