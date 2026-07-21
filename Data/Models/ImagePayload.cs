namespace Data.Models;

/// <summary>
/// Describes normalized image data ready to persist.
/// </summary>
public sealed record ImagePayload(
    byte[] Content,
    string ContentType);
