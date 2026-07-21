using Data.Entities;
using Infrastructure.Dto;
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
        long adminId,
        long nodeId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a manually managed routing rule for a remote node.
    /// </summary>
    Task<RoutingRuleEntity> UpdateAsync(
        long nodeId,
        long routingRuleId,
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
    /// Saves a full routing rule draft snapshot for a remote node.
    /// </summary>
    Task<List<RoutingRuleEntity>> SaveAsync(
        long adminId,
        long nodeId,
        IReadOnlyCollection<NodeRoutingRuleManualSaveItem> manualRules,
        IReadOnlyCollection<NodeRoutingRuleReadonlySaveItem> readonlyRules,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders manually managed routing rules for a remote node.
    /// </summary>
    Task<List<RoutingRuleEntity>> UpdateOrderAsync(
        long nodeId,
        IReadOnlyList<long> ruleIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a manually managed routing rule from a remote node.
    /// </summary>
    Task DeleteAsync(long nodeId, long routingRuleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes readonly routing rules from a node config template.
    /// </summary>
    Task SyncReadonlyFromTemplateAsync(
        long adminId,
        NodeEntity node,
        XrayConfig template,
        CancellationToken cancellationToken = default);
}
