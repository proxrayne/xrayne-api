using Node.Models;
using Grpc.Net.Client;

namespace Node.Grpc;

/// <summary>
/// Provides reusable gRPC channels for remote node endpoints.
/// </summary>
public interface INodeGrpcChannelProvider
{
    /// <summary>
    /// Creates or reuses a gRPC channel for the endpoint.
    /// </summary>
    GrpcChannel CreateChannel(NodeEndpoint endpoint);

    /// <summary>
    /// Removes and disposes the cached channel for the endpoint.
    /// </summary>
    void Invalidate(NodeEndpoint endpoint);
}
