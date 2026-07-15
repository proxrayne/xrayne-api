using Microsoft.Extensions.DependencyInjection;
using Node;
using Node.Configurations;
using Node.Services;

namespace Test.Infrastructure;

/// <summary>
/// Verifies remote node gRPC client dependency registration.
/// </summary>
public sealed class NodeDependencyInjectionTests
{
    /// <summary>
    /// Ensures split remote node client factories are registered.
    /// </summary>
    [Fact]
    public void AddNodes_registers_service_specific_client_factories()
    {
        var provider = new ServiceCollection()
            .AddNodes(new NodeOptions())
            .BuildServiceProvider();

        provider.GetRequiredService<INodeHealthClientFactory>().Should().NotBeNull();
        provider.GetRequiredService<INodeCoreClientFactory>().Should().NotBeNull();
        provider.GetRequiredService<INodeRuntimeConfigClientFactory>().Should().NotBeNull();
        provider.GetRequiredService<INodeLogClientFactory>().Should().NotBeNull();
        provider.GetRequiredService<INodeGeoResourceClientFactory>().Should().NotBeNull();
        provider.GetRequiredService<INodeCertificateClientFactory>().Should().NotBeNull();
        provider.GetRequiredService<INodeStreamClientFactory>().Should().NotBeNull();
    }
}
