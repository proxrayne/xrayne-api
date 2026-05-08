using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XRayne.Contracts.Configurations;

namespace XRayne.Contracts;

public static class DependencyInjection
{
    public static IServiceCollection AddContracts(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<XrayOptions>(configuration.GetSection("Xray"));

        return services;
    }
}
