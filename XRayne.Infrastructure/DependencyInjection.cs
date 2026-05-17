using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.Services.PanelSettings;
using XRayne.Infrastructure.Tasks;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ISystemInfoService>(sp => CreateSystemInfoService(sp.GetRequiredService<IProjectPathResolver>()));

        services.AddSingleton<IEventStreamManager, EventStreamManager>();
        services.AddSingleton<ICoreService, CoreService>();
        services.AddSingleton<ICoreStateMachine, CoreStateMachine>();
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();

        services.AddTransient<InstallCoreJob>();
        services.AddTransient<CoreOperationJob>();

        services.AddSingleton<IPanelSettingsAccessor, PanelSettingsAccessor>();
        services.AddSingleton<IProjectPathResolver, ProjectPathResolver>();
        services.AddHostedService<PanelSettingsBootstrapService>();

        return services;
    }

    private static SystemInfoService CreateSystemInfoService(IProjectPathResolver paths)
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsSystemInfoService(paths);
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSystemInfoService(paths);
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacOsSystemInfoService(paths);
        }

        throw new PlatformNotSupportedException(
            $"System information service is not supported on {RuntimeInformation.OSDescription}.");
    }
}
