namespace RemoteNode.Models;

/// <summary>
/// Describes the current xray-core state on the remote node.
/// </summary>
public sealed record NodeCoreStatus(
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    string Status);
