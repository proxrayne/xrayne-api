using Contracts.Enums;
using Data.Entities;
using Node.Models;

namespace Infrastructure.Services;

/// <summary>
/// Manages geo resources assigned to remote nodes.
/// </summary>
public interface INodeGeoResourceService
{
    /// <summary>
    /// Synchronizes panel geo resource metadata from a remote node.
    /// </summary>
    Task SynchronizeNodeAsync(NodeEntity node, CancellationToken cancellationToken = default);

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
        int updateInterval,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update geo resource status.
    /// </summary>
    Task UpdateStatusAsync(
        GeoResourceEntity resource,
        GeoResourceStatus status,
        string message,
        CancellationToken ct);

    /// <summary>
    /// Update geo resource status.
    /// </summary>
    Task UpdateStatusAsync(
        GeoResourceEntity resource,
        GeoResourceStatus status,
        CancellationToken ct);

    /// <summary>
    /// Updates a geo resource.
    /// </summary>
    Task<GeoResourceEntity> UpdateAsync(
        NodeEntity node,
        long id,
        string fileName,
        string? url,
        int? updateInterval,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a geo resource.
    /// </summary>
    Task DeleteAsync(NodeEntity node, long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads geo resource content from a remote node.
    /// </summary>
    Task<GeoResourceContent> DownloadResourceAsync(
        NodeEntity node,
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Download file by URL.
    /// </summary>
    Task<MemoryStream> DownloadAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Upload file to remote node.
    /// </summary>
    Task<GeoResourceEntity> UploadToNodeAsync(GeoResourceEntity entity, Stream content, CancellationToken cancellationToken = default);


    /// <summary>
    /// Schedules download or update auto-updated geo resources.
    /// </summary>
    Task ScheduleDownloadAutoUpdatesAsync(GeoResourceEntity entity, CancellationToken cancellationToken = default);
}
