using Data.Models;

namespace Data.Contracts;

/// <summary>
/// Provides read operations for stored images.
/// </summary>
public interface IImageRepository
{
    /// <summary>
    /// Gets image content by public key or returns null.
    /// </summary>
    Task<ImageContentReadModel?> GetContentByKeyOrDefaultAsync(
        string key,
        CancellationToken cancellationToken = default);
}
