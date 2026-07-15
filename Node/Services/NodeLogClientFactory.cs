using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates remote node log clients.
/// </summary>
public sealed class NodeLogClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeLogClientFactory
{
    /// <inheritdoc />
    public INodeLogClient Create(NodeEndpoint endpoint)
    {
        return new NodeLogClient(options, channelProvider, endpoint);
    }
}
