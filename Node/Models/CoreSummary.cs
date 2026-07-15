namespace Node.Models;

/// <summary>
/// Describes the current xray-core state on the remote node.
/// </summary>
public sealed record CoreSummary(
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    string Status);
