namespace RemoteNode.Models;

/// <summary>
/// Describes a node connection stream event.
/// </summary>
public sealed record NodeConnectionEvent(
    string Type,
    DateTimeOffset Timestamp,
    NodePingResponse? Ping,
    CoreStatusResponse? Core,
    InstallCoreStatusResponse? Install,
    RemoteLogStreamEvent? Log,
    long Sequence = 0,
    long DroppedCount = 0,
    string? Source = null);
