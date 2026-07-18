using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Services;

namespace Node;

/// <summary>
/// Registers remote node gRPC protocol clients.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds remote node gRPC clients and related protocol services.
    /// </summary>
    public static IServiceCollection AddNodes(this IServiceCollection services, NodeOptions options)
    {
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<INodeGrpcChannelProvider, NodeGrpcChannelProvider>();
        services.AddSingleton<INodeHealthClientFactory, NodeHealthClientFactory>();
        services.AddSingleton<INodeCoreClientFactory, NodeCoreClientFactory>();
        services.AddSingleton<INodeCoreInfrastructureClientFactory, NodeCoreInfrastructureClientFactory>();
        services.AddSingleton<INodeRuntimeConfigClientFactory, NodeRuntimeConfigClientFactory>();
        services.AddSingleton<INodeLogClientFactory, NodeLogClientFactory>();
        services.AddSingleton<INodeGeoResourceClientFactory, NodeGeoResourceClientFactory>();
        services.AddSingleton<INodeCertificateClientFactory, NodeCertificateClientFactory>();
        services.AddSingleton<INodeStreamClientFactory, NodeStreamClientFactory>();

        return services;
    }
}
