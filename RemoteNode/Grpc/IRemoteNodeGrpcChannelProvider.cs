using RemoteNode.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace RemoteNode.Grpc;

/// <summary>
/// Provides reusable gRPC clients backed by cached channels for remote node endpoints.
/// </summary>
public interface IRemoteNodeGrpcChannelProvider
{
    /// <summary>
    /// Creates a generated gRPC client for the endpoint using a cached channel.
    /// </summary>
    Proto.RemoteNodeService.RemoteNodeServiceClient CreateClient(RemoteNodeEndpoint endpoint);

    /// <summary>
    /// Removes and disposes the cached channel for the endpoint.
    /// </summary>
    void Invalidate(RemoteNodeEndpoint endpoint);
}
