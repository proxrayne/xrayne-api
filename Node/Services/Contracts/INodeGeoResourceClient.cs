using Node.Models;

namespace Node.Services;

/// <summary>
/// Sends authenticated geo resource requests to one remote node.
/// </summary>
public interface INodeGeoResourceClient
{
    /// <summary>
    /// Gets geo resources available on the remote node.
    /// </summary>
    Task<List<GeoResourceDto>> GetGeoResourcesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a geo resource file from the remote node.
    /// </summary>
    Task<GeoResourceContent> DownloadGeoResourceAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads or replaces a geo resource file on the remote node.
    /// </summary>
    Task<GeoResourceDto> UploadGeoResourceAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a geo resource file on the remote node.
    /// </summary>
    Task<GeoResourceDto> RenameGeoResourceAsync(
        string fileName,
        string newFilename,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a geo resource file from the remote node.
    /// </summary>
    Task DeleteGeoResourceAsync(string fileName, CancellationToken cancellationToken = default);
}
