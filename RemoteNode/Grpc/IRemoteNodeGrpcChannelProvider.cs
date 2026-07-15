using RemoteNode.Models;
using Grpc.Net.Client;

namespace RemoteNode.Grpc;

/// <summary>
/// Provides reusable gRPC channels for remote node endpoints.
/// </summary>
public interface IRemoteNodeGrpcChannelProvider
{
    /// <summary>
    /// Creates or reuses a gRPC channel for the endpoint.
    /// </summary>
    GrpcChannel CreateChannel(RemoteNodeEndpoint endpoint);

    /// <summary>
    /// Removes and disposes the cached channel for the endpoint.
    /// </summary>
    void Invalidate(RemoteNodeEndpoint endpoint);
}
