namespace XRayne.Node.Models;

public sealed record SystemStatusResponse(
    string Status,
    DateTimeOffset Timestamp);

public sealed record VersionResponse(
    string Service,
    string Version,
    string Environment);
