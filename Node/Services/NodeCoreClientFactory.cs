using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates remote node core clients.
/// </summary>
public sealed class NodeCoreClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeCoreClientFactory
{
    /// <inheritdoc />
    public INodeCoreClient Create(NodeEndpoint endpoint)
    {
        return new NodeCoreClient(options, channelProvider, endpoint);
    }
}
