namespace Node.Models;

/// <summary>
/// Describes a geo resource file stored on a remote node.
/// </summary>
public sealed record GeoResourceDto(
    string FileName,
    long SizeBytes,
    DateTimeOffset LastModifiedAt);

