using Contracts.Enums;

namespace Contracts.Models;

/// <summary>
/// Stores live remote xray-core state reported by a node.
/// </summary>
public sealed record RemoteNodeCoreState(
    long NodeId,
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    CoreStatus? Status);
