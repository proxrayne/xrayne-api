using Node.Models;

namespace Node.Services;

/// <summary>
/// Sends authenticated log snapshot requests to one remote node.
/// </summary>
public interface INodeLogClient
{
    /// <summary>
    /// Gets recent remote node log entries.
    /// </summary>
    Task<RemoteLogSnapshotResponse> GetLogsAsync(
        int? limit = null,
        CancellationToken cancellationToken = default);
}
