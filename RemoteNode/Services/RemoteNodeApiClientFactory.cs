using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Creates gRPC API clients for remote nodes.
/// </summary>
public sealed class RemoteNodeApiClientFactory(
    IOptions<RemoteNodeOptions> options) : IRemoteNodeApiClientFactory
{
    /// <inheritdoc />
    public IRemoteNodeApiClient Create(RemoteNodeEndpoint endpoint)
    {
        return new RemoteNodeApiClient(options, endpoint);
    }
}
