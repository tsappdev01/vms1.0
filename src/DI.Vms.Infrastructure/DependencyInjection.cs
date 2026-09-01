using DI.Vms.Application.Abstractions;
using DI.Vms.Infrastructure.Persistence;
using DI.Vms.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DI.Vms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVmsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VmsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Vms"),
                sql => sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

        services.Configure<IdProtectionOptions>(
            configuration.GetSection(IdProtectionOptions.SectionName));

        services.AddSingleton<IIdProtector, IdProtector>();
        services.AddScoped<IVisitNumberGenerator, VisitNumberGenerator>();
        services.AddScoped<IAuditWriter, AuditWriter>();

        return services;
    }
}
