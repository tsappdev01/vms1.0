using DI.Vms.Blazor.Components;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* So the reception PC can serve the desk from boot with nobody logged in. This checks
   whether the process really was started by the service control manager and does nothing
   when it was not, so `dotnet run` is unaffected. It also sets the content root to the
   executable's folder - a service starts in C:\Windows\System32 otherwise, and would
   find neither wwwroot nor appsettings.json. See docs/deployment.md. */
builder.Services.AddWindowsService(options => options.ServiceName = "DI VMS");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<VmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Vms")));

/* Singleton: the toolkit is a native context that is expensive to create and must not be
   initialised concurrently. The service serialises access internally. */
builder.Services.AddSingleton<CardReaderService>();

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
app.UseAntiforgery();

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
