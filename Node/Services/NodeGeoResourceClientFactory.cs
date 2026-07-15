using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates remote node geo resource clients.
/// </summary>
public sealed class NodeGeoResourceClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeGeoResourceClientFactory
{
    /// <inheritdoc />
    public INodeGeoResourceClient Create(NodeEndpoint endpoint)
    {
        return new NodeGeoResourceClient(options, channelProvider, endpoint);
    }
}
