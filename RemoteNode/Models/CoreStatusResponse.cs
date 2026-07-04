using RemoteNode.Enums;

namespace RemoteNode.Models;

/// <summary>
/// Describes the current Xray core installation and process state.
/// </summary>
public sealed record CoreStatusResponse(
    bool IsInstalled,
    CoreStatus? Status,
    bool IsInstalling,
    string? Version);
