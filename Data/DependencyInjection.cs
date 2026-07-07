using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Enums;
using Contracts.Utilities;
using Data.Contracts;
using Data.Implementations;

namespace Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(
        this IServiceCollection services,
        string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
                {
                    dataSourceBuilder.MapEnum<UserStatus>();
                    dataSourceBuilder.MapEnum<LimitResetStrategy>();
                    dataSourceBuilder.MapEnum<AdminPermission>();
                    dataSourceBuilder.MapEnum<SSHAuthType>();
                    dataSourceBuilder.MapEnum<CertificateMode>();

                    dataSourceBuilder
                        .EnableDynamicJson()
                        .ConfigureJsonOptions(XrayJsonSerializer.Options);

                });
            });
        });

        services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInboundRepository, InboundRepository>();
        services.AddScoped<IOutboundRepository, OutboundRepository>();
        services.AddScoped<INodeRepository, NodeRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IGeoResourceRepository, GeoResourceRepository>();
        services.AddScoped<IRoutingRuleRepository, RoutingRuleRepository>();
        services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
        services.AddSingleton<INodeConnectionStateStore, NodeConnectionStateStore>();
        services.AddSingleton<IRemoteNodeCoreStateStore, RemoteNodeCoreStateStore>();

        return services;
    }
}
