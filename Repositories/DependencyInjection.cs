using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Enums;
using Contracts.Utilities;
using Repositories.Contracts;
using Repositories.Implementations;

namespace Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(
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
                        .ConfigureJsonOptions(new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            WriteIndented = false
                        });

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
