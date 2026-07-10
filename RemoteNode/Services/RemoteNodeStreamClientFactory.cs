using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Creates SSE stream clients for remote nodes.
/// </summary>
public sealed class RemoteNodeStreamClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<RemoteNodeOptions> options) : IRemoteNodeStreamClientFactory
{
    /// <inheritdoc />
    public IRemoteNodeStreamClient Create(RemoteNodeEndpoint endpoint)
    {
        return new RemoteNodeStreamClient(httpClientFactory, options, endpoint);
    }
}
