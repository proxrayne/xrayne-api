using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XRayne.Infrastructure.Auth;
using XRayne.Infrastructure.Services;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<INetworkAddressService, NetworkAddressService>();
        services.AddScoped<ICoreService, CoreService>();

        return services;
    }
}
