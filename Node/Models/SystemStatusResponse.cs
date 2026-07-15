namespace Node.Models;

/// <summary>
/// Describes remote node system status.
/// </summary>
public sealed record SystemStatusResponse(
    DateTimeOffset Timestamp,
    SystemStats System);
