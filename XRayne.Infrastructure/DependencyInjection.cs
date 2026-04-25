using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // services.Configure<XRayneOptions>(configuration.GetSection("XRayne"));

        return services;
    }
}
