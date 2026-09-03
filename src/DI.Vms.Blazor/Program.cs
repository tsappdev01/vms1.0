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

/* Brings the database up to what the code expects on startup, so neither a fresh
   database nor UATWEB01 - which already exists, holding tables from an earlier design -
   needs a separate step. Both are deliberately not EnsureCreated / HasData; see each. */
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<VmsDbContext>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await using var db = await factory.CreateDbContextAsync();

    await DbBootstrapper.EnsureSchemaAsync(db, logger);
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
