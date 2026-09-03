using DI.Vms.Portal.Components;
using DI.Vms.Portal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

/* Interactive server rendering, so the dashboard and the evacuation list update over
   the existing circuit rather than by polling. A security dashboard showing a
   30-second-old occupancy figure is misleading in exactly the situation that matters. */

builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<CardReaderClient>();

builder.Services.AddHttpClient<VmsApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    /* The API's development certificate is self-signed. Trusting it is a development
       convenience only, so it is gated on the environment rather than a config flag
       someone could set in production by accident. */
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
