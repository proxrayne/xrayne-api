using Node.Models;

namespace Node.Services;

/// <summary>
/// Sends authenticated runtime configuration synchronization requests to one remote node.
/// </summary>
public interface INodeRuntimeConfigClient
{
    /// <summary>
    /// Adds an inbound to the remote node runtime when xray-core is started.
    /// </summary>
    Task AddInboundAsync(SyncInboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an inbound in the remote node runtime when xray-core is started.
    /// </summary>
    Task UpdateInboundAsync(string id, SyncInboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an inbound from the remote node runtime when xray-core is started.
    /// </summary>
    Task DeleteInboundAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an outbound to the remote node runtime when xray-core is started.
    /// </summary>
    Task AddOutboundAsync(SyncOutboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an outbound in the remote node runtime when xray-core is started.
    /// </summary>
    Task UpdateOutboundAsync(string id, SyncOutboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an outbound from the remote node runtime when xray-core is started.
    /// </summary>
    Task DeleteOutboundAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces routing rules in the remote node runtime when xray-core is started.
    /// </summary>
    Task SyncRoutingRulesAsync(SyncRoutingRulesRequest request, CancellationToken cancellationToken = default);
}
