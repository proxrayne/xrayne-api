using Contracts.Enums;

namespace Contracts.Models;

/// <summary>
/// Stores live remote xray-core state reported by a node.
/// </summary>
public sealed record NodeCoreState(
    long NodeId,
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    CoreStatus? Status,
    DateTimeOffset? StartedAt,
    TimeSpan? Uptime);
