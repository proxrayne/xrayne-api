using Microsoft.Extensions.DependencyInjection;
using XRayne.Core.Services;
using XRayne.Core.Tasks;

namespace XRayne.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ICoreService, CoreService>();

        services.AddTransient<InstallCoreJob>();

        return services;
    }
}
