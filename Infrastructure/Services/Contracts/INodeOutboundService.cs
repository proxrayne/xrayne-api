using Data.Entities;
using Xray.Config.Models;

namespace Infrastructure.Services;

/// <summary>
/// Manages outbounds assigned to remote nodes.
/// </summary>
public interface INodeOutboundService
{
    /// <summary>
    /// Gets outbounds assigned to a remote node.
    /// </summary>
    Task<List<OutboundEntity>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one outbound assigned to a remote node.
    /// </summary>
    Task<OutboundEntity> GetByNodeAndIdAsync(
        long nodeId,
        int outboundId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a manually managed outbound for a remote node.
    /// </summary>
    Task<OutboundEntity> CreateAsync(
        Guid adminId,
        long nodeId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a manually managed outbound for a remote node.
    /// </summary>
    Task<OutboundEntity> UpdateAsync(
        long nodeId,
        int outboundId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates enabled state for any outbound assigned to a remote node.
    /// </summary>
    Task<OutboundEntity> UpdateEnabledAsync(
        long nodeId,
        int outboundId,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a manually managed outbound from a remote node.
    /// </summary>
    Task DeleteAsync(long nodeId, int outboundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes readonly outbounds from a node config template.
    /// </summary>
    Task SyncReadonlyFromTemplateAsync(
        Guid adminId,
        NodeEntity node,
        XrayConfig template,
        CancellationToken cancellationToken = default);
}
