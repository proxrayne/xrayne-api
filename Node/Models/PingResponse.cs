namespace Node.Models;

/// <summary>
/// Describes the current remote node telemetry returned to the panel.
/// </summary>
public sealed record PingResponse(
    string NodeVersion,
    string Environment,
    TimeSpan Uptime,
    CoreSummary Core);
