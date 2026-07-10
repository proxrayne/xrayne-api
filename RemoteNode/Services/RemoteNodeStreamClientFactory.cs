using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Grpc;
using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Creates gRPC stream clients for remote nodes.
/// </summary>
public sealed class RemoteNodeStreamClientFactory(
    IOptions<RemoteNodeOptions> options,
    IRemoteNodeGrpcChannelProvider channelProvider) : IRemoteNodeStreamClientFactory
{
    /// <inheritdoc />
    public IRemoteNodeStreamClient Create(RemoteNodeEndpoint endpoint)
    {
        return new RemoteNodeStreamClient(options, channelProvider, endpoint);
    }
}
