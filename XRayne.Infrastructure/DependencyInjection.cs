using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XRayne.Infrastructure.Services;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICoreService, CoreService>();

        return services;
    }
}
