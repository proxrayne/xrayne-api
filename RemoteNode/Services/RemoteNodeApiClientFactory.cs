using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Creates HTTP API clients for remote nodes.
/// </summary>
public sealed class RemoteNodeApiClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<RemoteNodeOptions> options) : IRemoteNodeApiClientFactory
{
    /// <inheritdoc />
    public IRemoteNodeApiClient Create(RemoteNodeEndpoint endpoint)
    {
        return new RemoteNodeApiClient(httpClientFactory, options, endpoint);
    }
}
