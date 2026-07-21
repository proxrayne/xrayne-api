using Data.Contracts;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core read operations for stored images.
/// </summary>
public sealed class ImageRepository(AppDbContext context) : IImageRepository
{
    /// <inheritdoc />
    public Task<ImageContentReadModel?> GetContentByKeyOrDefaultAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return context.Images
            .AsNoTracking()
            .Where(image => image.Key == key)
            .Select(image => new ImageContentReadModel(
                image.Content,
                image.ContentType,
                image.Version))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
