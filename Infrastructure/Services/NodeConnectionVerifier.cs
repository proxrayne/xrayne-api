using RemoteNode.Models;
using RemoteNode.Services;
using Repositories.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Verifies a node connection by calling its authenticated ping endpoint.
/// </summary>
public sealed class NodeConnectionVerifier(IRemoteNodeApiClientFactory apiClientFactory) : INodeConnectionVerifier
{
    /// <inheritdoc />
    public async Task<NodeConnectionVerificationResult> VerifyAsync(
        NodeEntity node,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var endpoint = new RemoteNodeEndpoint(node.Id, node.Address, node.ApiPort, apiKey);
        var ping = await apiClientFactory.Create(endpoint).PingAsync(cancellationToken);

        return new NodeConnectionVerificationResult(ping.Core.Version, ping.Timestamp);
    }
}
