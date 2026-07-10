using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Creates gRPC stream clients for remote nodes.
/// </summary>
public interface IRemoteNodeStreamClientFactory
{
    /// <summary>
    /// Creates a stream client for one remote node endpoint.
    /// </summary>
    IRemoteNodeStreamClient Create(RemoteNodeEndpoint endpoint);
}
