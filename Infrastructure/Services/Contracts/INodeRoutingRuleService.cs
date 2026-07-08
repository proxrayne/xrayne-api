using Data.Entities;
using Xray.Config.Models;

namespace Infrastructure.Services;

/// <summary>
/// Manages routing rules assigned to remote nodes.
/// </summary>
public interface INodeRoutingRuleService
{
    /// <summary>
    /// Gets routing rules assigned to a remote node.
    /// </summary>
    Task<List<RoutingRuleEntity>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one routing rule assigned to a remote node.
    /// </summary>
    Task<RoutingRuleEntity> GetByNodeAndIdAsync(
        long nodeId,
        long routingRuleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a manually managed routing rule for a remote node.
    /// </summary>
    Task<RoutingRuleEntity> CreateAsync(
        Guid adminId,
        long nodeId,
        string tag,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a manually managed routing rule for a remote node.
    /// </summary>
    Task<RoutingRuleEntity> UpdateAsync(
        long nodeId,
        long routingRuleId,
        string tag,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates enabled state for any routing rule assigned to a remote node.
    /// </summary>
    Task<RoutingRuleEntity> UpdateEnabledAsync(
        long nodeId,
        long routingRuleId,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders manually managed routing rules for a remote node.
    /// </summary>
    Task<List<RoutingRuleEntity>> UpdateOrderAsync(
        long nodeId,
        IReadOnlyList<long> routingRuleIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a manually managed routing rule from a remote node.
    /// </summary>
    Task DeleteAsync(long nodeId, long routingRuleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes readonly routing rules from a node config template.
    /// </summary>
    Task SyncReadonlyFromTemplateAsync(
        Guid adminId,
        NodeEntity node,
        XrayConfig template,
        CancellationToken cancellationToken = default);
}
