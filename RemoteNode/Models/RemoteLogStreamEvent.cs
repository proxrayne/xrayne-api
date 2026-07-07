namespace RemoteNode.Models;

/// <summary>
/// Describes a remote log stream event.
/// </summary>
public sealed record RemoteLogStreamEvent(
    string Type,
    IReadOnlyList<RemoteLogEntry>? Entries,
    RemoteLogEntry? Entry);
