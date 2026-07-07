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
    RemoteLogStreamEvent? Log);
