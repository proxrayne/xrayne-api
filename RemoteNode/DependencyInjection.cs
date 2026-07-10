using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Services;

namespace RemoteNode;

/// <summary>
/// Registers remote node API protocol clients.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds remote node API clients and related protocol services.
    /// </summary>
    public static IServiceCollection AddRemoteNodes(this IServiceCollection services, RemoteNodeOptions options)
    {
        services.AddSingleton(Options.Create(options));
        services.AddHttpClient("remote-node");
        services.AddSingleton<IRemoteNodeApiClientFactory, RemoteNodeApiClientFactory>();
        services.AddSingleton<IRemoteNodeStreamClientFactory, RemoteNodeStreamClientFactory>();

        return services;
    }
}
