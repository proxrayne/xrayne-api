using Node.Models;

namespace Infrastructure.Services;

/// <summary>
/// Stores live remote node logs in memory.
/// </summary>
public interface INodeLogStore
{
    /// <summary>
    /// Gets a normalized entry limit.
    /// </summary>
    int NormalizeLimit(int? limit);

    /// <summary>
    /// Gets recent xray-core log entries for a node.
    /// </summary>
    IReadOnlyList<RemoteLogEntry> Get(long nodeId, int? limit = null);

    /// <summary>
    /// Appends entries to the live buffer.
    /// </summary>
    void Append(long nodeId, RemoteLogStreamEvent logEvent);

    /// <summary>
    /// Removes all logs for a node.
    /// </summary>
    void Remove(long nodeId);
}
