using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XRayne.Infrastructure.Configuration;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddXRayneInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<XRayneOptions>(configuration.GetSection("XRayne"));

        return services;
    }
}
