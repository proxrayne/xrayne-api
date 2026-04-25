using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xray.Core;
using XRayne.Core.Configurations;

namespace XRayne.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<XrayInstanceConfig>(configuration.GetSection("Xray"));

        services.AddSingleton<IXrayCore>((provider) =>
        {
            var configOptions = provider.GetRequiredService<IOptions<XrayInstanceConfig>>();

            if (configOptions.Value.UseProcessCore)
            {
                return new XrayProcessCore(new XrayProcessOptions()
                {
                    WorkingDirectory = configOptions.Value.Directory,
                });
            }

            return new XrayLibCore(new XrayLibOptions()
            {
                LibPath = Path.Combine(configOptions.Value.Directory, configOptions.Value.FileName)
            });
        });

        return services;
    }
}
