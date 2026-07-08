using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Contracts.Configurations;
using Contracts.Values;
using Infrastructure.Dto;
using Infrastructure.Services;
using Infrastructure.Tasks;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<NodeConnectionOptions>(configuration.GetSection("NodeConnection"));
        services.Configure<NodeLogOptions>(configuration.GetSection("NodeLogs"));
        services.TryAddSingleton(_ => PanelSettings.Parse(configuration));
        services.AddDataProtection();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ISystemInfoService>(_ =>
            new SystemInfoService(new SystemInfoOptions(
                PathProvider.Paths.Root,
                PathProvider.Paths.DownloadsDirectory
            ))
        );

        services.AddSingleton<IEventStreamManager, EventStreamManager>();
        services.AddSingleton<INodeLogStore, NodeLogStore>();
        services.AddSingleton<INodeSecretService, NodeSecretService>();
        services.AddSingleton<INodeProvisionStateMachine, NodeProvisionStateMachine>();
        services.AddSingleton<INodeReconnectPolicy, NodeReconnectPolicy>();
        services.AddSingleton<INodeImageReleaseResolver, NodeImageReleaseResolver>();
        services.AddSingleton<INodeCoreConfigBuilder, NodeCoreConfigBuilder>();
        services.AddSingleton<INodeConnectionVerifier, NodeConnectionVerifier>();
        services.AddSingleton<IRemoteNodeConnectionManager, RemoteNodeConnectionManager>();
        services.AddSingleton<IRemoteNodeProvisioner, RemoteNodeProvisioner>();
        services.AddHostedService<NodeConnectionHostedService>();
        services.AddSingleton<ICoreService, CoreService>();
        services.AddSingleton<ICoreStateMachine, CoreStateMachine>();
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddTransient<InstallCoreJob>();
        services.AddTransient<CoreOperationJob>();
        services.AddTransient<NodeProvisionJob>();

        services.AddScoped<IAppSettingsService, AppSettingsService>();
        services.AddScoped<INodeService, NodeService>();
        services.AddScoped<INodeInboundService, NodeInboundService>();
        services.AddScoped<INodeOutboundService, NodeOutboundService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<INodeCertificateService, NodeCertificateService>();
        services.AddScoped<IGeoResourceService, GeoResourceService>();
        services.AddScoped<INodeRoutingRuleService, NodeRoutingRuleService>();

        return services;
    }
}
