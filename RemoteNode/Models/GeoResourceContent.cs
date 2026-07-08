namespace RemoteNode.Models;

/// <summary>
/// Contains downloaded geo resource file content.
/// </summary>
public sealed record GeoResourceContent(
    string FileName,
    byte[] Content);

