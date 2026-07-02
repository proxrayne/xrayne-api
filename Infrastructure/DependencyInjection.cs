using System.Runtime.InteropServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.Tasks;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<NodeConnectionOptions>(configuration.GetSection("NodeConnection"));
        services.TryAddSingleton(_ => PanelSettings.Parse(configuration));
        services.AddDataProtection();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ISystemInfoService>(sp => CreateSystemInfoService());

        services.AddSingleton<IEventStreamManager, EventStreamManager>();
        services.AddSingleton<INodeSecretService, NodeSecretService>();
        services.AddSingleton<INodeProvisionStateMachine, NodeProvisionStateMachine>();
        services.AddSingleton<INodeReconnectPolicy, NodeReconnectPolicy>();
        services.AddSingleton<INodeImageReleaseResolver, NodeImageReleaseResolver>();
        services.AddSingleton<INodeConnectionVerifier, NodeConnectionVerifier>();
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
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IGeoResourceService, GeoResourceService>();
        services.AddScoped<IRoutingRuleService, RoutingRuleService>();

        return services;
    }

    private static SystemInfoService CreateSystemInfoService()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsSystemInfoService();
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSystemInfoService();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacOsSystemInfoService();
        }

        throw new PlatformNotSupportedException(
            $"System information service is not supported on {RuntimeInformation.OSDescription}.");
    }
}
