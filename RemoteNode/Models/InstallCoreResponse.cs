namespace RemoteNode.Models;

/// <summary>
/// Describes an accepted Xray core installation job.
/// </summary>
public sealed record InstallCoreResponse(
    string JobId,
    string Version,
    string Status);
