using DI.Vms.Blazor.Components;
using DI.Vms.Blazor.Data;
using DI.Vms.Blazor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<VmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Vms")));

/* Singleton: the toolkit is a native context that is expensive to create and must not be
   initialised concurrently. The service serialises access internally. */
builder.Services.AddSingleton<CardReaderService>();

var app = builder.Build();

/* Applies the schema on startup so a fresh database needs no separate step. Migrations
   would be the right answer once the schema is stable and shared. */
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<VmsDbContext>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();

    // Additive, and runs every start, so the entity list can be changed in code and
    // reaches an existing database - which HasData would not.
    await EntitySeeder.SyncAsync(db, logger);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
