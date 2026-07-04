using RemoteNode.Models;
using RemoteNode.Services;
using Repositories.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Verifies a node connection by calling its authenticated ping endpoint.
/// </summary>
public sealed class NodeConnectionVerifier(
    IRemoteNodeApiClientFactory apiClientFactory,
    IRemoteNodeTelemetryCache telemetryCache) : INodeConnectionVerifier
{
    /// <inheritdoc />
    public async Task<NodeConnectionVerificationResult> VerifyAsync(
        NodeEntity node,
        string apiKey,
        CancellationToken cancellationToken)
    {
        var endpoint = new RemoteNodeEndpoint(node.Id, node.Address, node.ApiPort, apiKey);
        var ping = await apiClientFactory.Create(endpoint).PingAsync(cancellationToken);
        var verifiedAt = DateTimeOffset.UtcNow;

        telemetryCache.Set(new RemoteNodeConnectionSnapshot(
            node.Id,
            RemoteNodeConnectionState.Connected,
            verifiedAt,
            verifiedAt,
            verifiedAt,
            0,
            null,
            ping));

        return new NodeConnectionVerificationResult(verifiedAt);
    }
}
