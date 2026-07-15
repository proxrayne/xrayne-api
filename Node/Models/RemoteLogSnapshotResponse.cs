namespace Node.Models;

/// <summary>
/// Response model for recent remote log entries.
/// </summary>
public sealed record RemoteLogSnapshotResponse(
    int Limit,
    IReadOnlyList<RemoteLogEntry> Entries);
