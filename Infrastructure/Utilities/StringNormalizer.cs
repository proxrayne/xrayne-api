using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Utilities;

public static class StringNormalizer
{
    public static string NormalizeFileName(string fileName, int maxLength = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ValidationException("Geo resource file name is required.");
        }

        var normalized = fileName.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ValidationException($"Geo resource file name must be {maxLength} characters or fewer.");
        }

        if (!string.Equals(Path.GetFileName(normalized), normalized, StringComparison.Ordinal)
            || normalized.Contains('/', StringComparison.Ordinal)
            || normalized.Contains('\\', StringComparison.Ordinal)
            || normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ValidationException("Geo resource file name is invalid.");
        }

        return normalized;
    }

    public static string NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ValidationException("Geo resource URL is required.");
        }

        var normalized = url.Trim();
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            throw new ValidationException("Geo resource URL must be an absolute HTTP or HTTPS URL.");
        }

        return normalized;
    }
}