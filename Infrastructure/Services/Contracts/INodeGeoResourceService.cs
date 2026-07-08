using Data.Entities;
using RemoteNode.Models;

namespace Infrastructure.Services;

/// <summary>
/// Manages geo resources assigned to remote nodes.
/// </summary>
public interface INodeGeoResourceService
{
    /// <summary>
    /// Synchronizes panel geo resource metadata from a remote node.
    /// </summary>
    Task<List<GeoResourceEntity>> SynchronizeNodeAsync(
        Guid adminId,
        NodeEntity node,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets geo resources assigned to a node.
    /// </summary>
    Task<List<GeoResourceEntity>> GetAllAsync(
        Guid adminId,
        NodeEntity node,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a static geo resource from uploaded file content.
    /// </summary>
    Task<GeoResourceEntity> CreateFileAsync(
        Guid adminId,
        NodeEntity node,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an auto-updated geo resource from a remote URL.
    /// </summary>
    Task<GeoResourceEntity> CreateAutoUpdateAsync(
        Guid adminId,
        NodeEntity node,
        string fileName,
        string url,
        string cronTemplate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a geo resource.
    /// </summary>
    Task<GeoResourceEntity> UpdateAsync(
        Guid adminId,
        NodeEntity node,
        long id,
        string fileName,
        string? url,
        string? cronTemplate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a geo resource.
    /// </summary>
    Task DeleteAsync(Guid adminId, NodeEntity node, long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads geo resource content from a remote node.
    /// </summary>
    Task<GeoResourceContent> DownloadAsync(
        Guid adminId,
        NodeEntity node,
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes due auto-updated geo resources.
    /// </summary>
    Task RefreshDueAutoUpdatesAsync(CancellationToken cancellationToken = default);
}

