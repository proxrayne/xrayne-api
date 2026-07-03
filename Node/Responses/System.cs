namespace Node.Responses;

/// <summary>
/// Describes remote node system status.
/// </summary>
public sealed record SystemStatusResponse(
    string Status,
    DateTimeOffset Timestamp);

/// <summary>
/// Describes remote node API version information.
/// </summary>
public sealed record VersionResponse(
    string Service,
    string Version,
    string Environment);
