namespace Node.Models;

/// <summary>
/// Describes one live remote log entry.
/// </summary>
public sealed record RemoteLogEntry(
    string Id,
    DateTimeOffset Timestamp,
    string Level,
    string Message);
