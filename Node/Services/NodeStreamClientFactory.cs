using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates gRPC stream clients for remote nodes.
/// </summary>
public sealed class NodeStreamClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeStreamClientFactory
{
    /// <inheritdoc />
    public INodeStreamClient Create(NodeEndpoint endpoint)
    {
        return new NodeStreamClient(options, channelProvider, endpoint);
    }
}
