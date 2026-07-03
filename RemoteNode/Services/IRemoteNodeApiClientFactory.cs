using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Creates API clients for authenticated remote node endpoints.
/// </summary>
public interface IRemoteNodeApiClientFactory
{
    /// <summary>
    /// Creates a client for the provided remote node endpoint.
    /// </summary>
    IRemoteNodeApiClient Create(RemoteNodeEndpoint endpoint);
}
