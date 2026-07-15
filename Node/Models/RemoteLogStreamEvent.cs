namespace Node.Models;

/// <summary>
/// Describes a remote log stream event.
/// </summary>
public sealed record RemoteLogStreamEvent(
    string Type,
    IReadOnlyList<RemoteLogEntry>? Entries,
    RemoteLogEntry? Entry,
    long Sequence = 0,
    long DroppedCount = 0,
    string? Source = null);
