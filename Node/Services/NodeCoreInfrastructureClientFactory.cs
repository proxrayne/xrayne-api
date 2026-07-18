using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates remote node xray-core utility clients.
/// </summary>
public sealed class NodeCoreInfrastructureClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeCoreInfrastructureClientFactory
{
    /// <inheritdoc />
    public INodeCoreInfrastructureClient Create(NodeEndpoint endpoint)
    {
        return new NodeCoreInfrastructureClient(options, channelProvider, endpoint);
    }
}
