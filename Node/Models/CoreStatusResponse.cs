using Node.Enums;

namespace Node.Models;

/// <summary>
/// Describes the current Xray core installation and process state.
/// </summary>
public sealed record CoreStatusResponse(
    bool IsInstalled,
    RemoteCoreStatus? Status,
    bool IsInstalling,
    string? Version,
    DateTimeOffset? StartedAt,
    TimeSpan? Uptime);
