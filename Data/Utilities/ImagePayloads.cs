using Contracts.Exceptions;
using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Http;

namespace Data.Utilities;

/// <summary>
/// Normalizes, validates, and applies persisted image payloads.
/// </summary>
public static class ImagePayloads
{
    private static readonly HashSet<string> SupportedContentTypes =
    [
        "image/png",
        "image/jpeg",
        "image/webp",
        "image/gif"
    ];

    /// <summary>
    /// Normalizes an uploaded image file for persistence.
    /// </summary>
    public static async Task<ImagePayload?> NormalizeForWriteAsync(
        IFormFile? file,
        bool required,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            return required
                ? throw new BadRequestException("Image file is required.")
                : null;
        }

        if (file.Length == 0)
        {
            throw new BadRequestException("Image file cannot be empty.");
        }

        if (file.Length > int.MaxValue)
        {
            throw new BadRequestException("Image file is too large.");
        }

        var contentType = NormalizeContentType(file.ContentType);
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new BadRequestException("Image content type is required.");
        }

        ValidateSupportedContentType(contentType);

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream((int)file.Length);
        await stream.CopyToAsync(memory, cancellationToken);

        return new ImagePayload(memory.ToArray(), contentType);
    }

    /// <summary>
    /// Applies a normalized payload to an existing image and bumps the version when data changes.
    /// </summary>
    public static bool ApplyPayload(ImageEntity existing, ImagePayload? payload)
    {
        if (payload is null)
        {
            return false;
        }

        if (existing.Content.SequenceEqual(payload.Content)
            && string.Equals(existing.ContentType, payload.ContentType, StringComparison.Ordinal))
        {
            return false;
        }

        if (existing.Version == long.MaxValue)
        {
            throw new BadRequestException("Image version cannot be incremented.");
        }

        existing.Content = payload.Content;
        existing.ContentType = payload.ContentType;
        existing.Version = Math.Max(existing.Version, 1) + 1;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        return true;
    }

    private static string? NormalizeContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        var normalized = contentType.Trim().ToLowerInvariant();
        return normalized == "image/jpg" ? "image/jpeg" : normalized;
    }

    private static void ValidateSupportedContentType(string contentType)
    {
        if (!SupportedContentTypes.Contains(contentType))
        {
            throw new BadRequestException("Image content type is not supported.");
        }
    }
}
