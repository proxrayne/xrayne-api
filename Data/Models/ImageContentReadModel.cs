namespace Data.Models;

/// <summary>
/// Describes the minimum data needed to return an image HTTP response.
/// </summary>
public sealed record ImageContentReadModel(
    byte[] Content,
    string ContentType,
    long Version);
