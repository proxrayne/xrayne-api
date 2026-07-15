using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates remote node health clients.
/// </summary>
public sealed class NodeHealthClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeHealthClientFactory
{
    /// <inheritdoc />
    public INodeHealthClient Create(NodeEndpoint endpoint)
    {
        return new NodeHealthClient(options, channelProvider, endpoint);
    }
}
