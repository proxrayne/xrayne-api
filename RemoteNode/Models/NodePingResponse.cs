namespace RemoteNode.Models;

/// <summary>
/// Describes the current remote node telemetry returned to the panel.
/// </summary>
public sealed record NodePingResponse(
    string NodeVersion,
    string Environment,
    TimeSpan Uptime,
    NodeCoreStatus Core);
