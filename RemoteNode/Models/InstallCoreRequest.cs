namespace RemoteNode.Models;

/// <summary>
/// Requests installation of a specific Xray core version.
/// </summary>
public sealed record InstallCoreRequest(string? Version);
