using Contracts.Configurations;
using Contracts.Values;
using Infrastructure.Dto;
using Infrastructure.Services;
using Infrastructure.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        services.AddSingleton<IPanelRestartService, PanelRestartService>();

        services.AddSingleton<IEventStreamManager, EventStreamManager>();
        services.AddSingleton<INodeLogStore, NodeLogStore>();
        services.AddSingleton<INodeSecretService, NodeSecretService>();
        services.AddSingleton<INodeProvisionStateMachine, NodeProvisionStateMachine>();
        services.AddSingleton<INodeReconnectPolicy, NodeReconnectPolicy>();
        services.AddSingleton<INodeImageReleaseResolver, NodeImageReleaseResolver>();
        services.AddSingleton<INodeCoreConfigBuilder, NodeCoreConfigBuilder>();
        services.AddSingleton<INodeConnectionVerifier, NodeConnectionVerifier>();
        services.AddSingleton<INodeConnectionManager, NodeConnectionManager>();
        services.AddSingleton<INodeProvisioner, NodeProvisioner>();
        services.AddHostedService<NodeConnectionHostedService>();
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);

        services.AddTransient<NodeProvisionJob>();
        services.AddTransient<GeoResourceSyncJob>();
        services.AddTransient<UpdateGeoResourcesJob>();
        services.AddTransient<GeoResourceDownloadJob>();
        services.AddTransient<GeoResourceUploadJob>();

        services.AddScoped<IAppSettingsService, AppSettingsService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<INodeInboundService, NodeInboundService>();
        services.AddScoped<INodeOutboundService, NodeOutboundService>();
        services.AddScoped<INodeCertificateService, NodeCertificateService>();
        services.AddScoped<INodeGeoResourceService, NodeGeoResourceService>();
        services.AddScoped<INodeRoutingRuleService, NodeRoutingRuleService>();
        services.AddScoped<INodeCoreService, NodeCoreService>();
        services.AddScoped<ITempFileStorage, TempFileStorage>();

        services.AddHttpClient("geo-resources");

        return services;
    }
}
