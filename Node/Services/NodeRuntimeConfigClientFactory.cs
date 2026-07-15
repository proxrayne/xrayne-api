using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates remote node runtime configuration clients.
/// </summary>
public sealed class NodeRuntimeConfigClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeRuntimeConfigClientFactory
{
    /// <inheritdoc />
    public INodeRuntimeConfigClient Create(NodeEndpoint endpoint)
    {
        return new NodeRuntimeConfigClient(options, channelProvider, endpoint);
    }
}
