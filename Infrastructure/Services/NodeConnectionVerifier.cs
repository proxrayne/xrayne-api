using Contracts.Enums;
using Contracts.Models;
using Contracts.Utilities;
using Infrastructure.Dto;
using RemoteNode.Models;
using RemoteNode.Services;
using Data.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Verifies a node connection by calling its authenticated ping endpoint.
/// </summary>
public sealed class NodeConnectionVerifier(
    IRemoteNodeApiClientFactory apiClientFactory,
    INodeConnectionStateStore connectionStates,
    IRemoteNodeCoreStateStore coreStates) : INodeConnectionVerifier
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

        connectionStates.Set(new NodeConnectionState(
            node.Id,
            NodeConnectionStatus.Connected,
            ping.NodeVersion,
            verifiedAt - ping.Uptime));
        coreStates.Set(new RemoteNodeCoreState(
            node.Id,
            ping.Core.IsInstalled,
            ping.Core.IsRunning,
            ping.Core.Version,
            TryMapCoreStatus(ping.Core.Status),
            null,
            null));

        return new NodeConnectionVerificationResult(verifiedAt);
    }

    private static CoreStatus? TryMapCoreStatus(string? status)
    {
        return Enum.TryParse<CoreStatus>(status, ignoreCase: true, out var result)
            ? result
            : null;
    }
}
