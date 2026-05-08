using Microsoft.Extensions.DependencyInjection;
using XRayne.Core.Services;

namespace XRayne.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ICoreService, CoreService>();
        services.AddSingleton<ICoreDownloadService, CoreDownloadService>();

        return services;
    }
}
