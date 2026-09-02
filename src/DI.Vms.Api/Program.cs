using DI.Vms.Api.Auth;
using DI.Vms.Api.Endpoints;
using DI.Vms.Application.Abstractions;
using DI.Vms.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVmsInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddPolicy("portal", policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

/* Real authentication is Entra ID (BRD 23) and needs an app registration that does not
   exist yet. Until it does, a development stand-in supplies the operator so the
   authorisation rules can be exercised. It must never reach production. */
var authMode = builder.Configuration["Auth:Mode"] ?? "Development";

if (string.Equals(authMode, "Development", StringComparison.OrdinalIgnoreCase))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException(
            "Auth:Mode is 'Development' in the Production environment. Configure Entra ID " +
            "before deploying: the development stand-in trusts a request header for identity.");
    }

    builder.Services.AddScoped<ICurrentUser, DevCurrentUser>();
}
else
{
    throw new NotSupportedException(
        $"Auth:Mode '{authMode}' is not implemented yet. Entra ID integration is outstanding.");
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("portal");

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("Health");

app.MapVisitEndpoints();
app.MapVisitorEndpoints();
app.MapDashboardEndpoints();
app.MapMasterDataEndpoints();
app.MapReportEndpoints();

app.Run();
