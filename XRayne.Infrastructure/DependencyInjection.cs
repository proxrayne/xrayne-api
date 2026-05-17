using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ISystemInfoService>(sp => CreateSystemInfoService());

        services.AddSingleton<IEventStreamManager, EventStreamManager>();
        services.AddSingleton<ICoreService, CoreService>();
        services.AddSingleton<ICoreStateMachine, CoreStateMachine>();
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();

        services.AddTransient<InstallCoreJob>();
        services.AddTransient<CoreOperationJob>();

        services.AddSingleton<ISettingsService, SettingsService>();

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
