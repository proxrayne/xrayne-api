using Data.Entities;
using Xray.Config.Models;

namespace Infrastructure.Services;

/// <summary>
/// Manages inbounds assigned to remote nodes.
/// </summary>
public interface INodeInboundService
{
    /// <summary>
    /// Gets inbounds assigned to a remote node.
    /// </summary>
    Task<List<InboundEntity>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one inbound assigned to a remote node.
    /// </summary>
    Task<InboundEntity> GetByNodeAndIdAsync(
        long nodeId,
        long inboundId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a manually managed inbound for a remote node.
    /// </summary>
    Task<InboundEntity> CreateAsync(
        Guid adminId,
        long nodeId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a manually managed inbound for a remote node.
    /// </summary>
    Task<InboundEntity> UpdateAsync(
        long nodeId,
        long inboundId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates enabled state for any inbound assigned to a remote node.
    /// </summary>
    Task<InboundEntity> UpdateEnabledAsync(
        long nodeId,
        long inboundId,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a manually managed inbound from a remote node.
    /// </summary>
    Task DeleteAsync(long nodeId, long inboundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes readonly inbounds from a node config template.
    /// </summary>
    Task SyncReadonlyFromTemplateAsync(
        Guid adminId,
        NodeEntity node,
        XrayConfig template,
        CancellationToken cancellationToken = default);
}
