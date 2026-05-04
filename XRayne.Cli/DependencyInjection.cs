using Microsoft.Extensions.DependencyInjection;
using XRayne.Cli.Commands;
using XRayne.Cli.Commands.Admin;
using XRayne.Cli.Commands.Xray;
using XRayne.Cli.Output;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCliActions(this IServiceCollection services)
    {
        services.AddSingleton<ICliConsole, CliConsole>();

        services.AddSingleton<RootCommandFactory>();
        services.AddSingleton<VersionCommand>();
        services.AddSingleton<XrayCommand>();
        services.AddSingleton<XrayStartCommand>();
        services.AddSingleton<AdminCommand>();
        services.AddSingleton<AdminCreateCommand>();

        return services;
    }
}
