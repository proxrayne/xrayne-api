namespace Node.Models;

/// <summary>
/// Contains downloaded geo resource file content.
/// </summary>
public sealed record GeoResourceContent(
    string FileName,
    IAsyncEnumerable<byte[]> Content);
