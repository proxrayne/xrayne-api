namespace Infrastructure.Values;

/// <summary>
/// Provides panel-local stream keys for remote node runtime events.
/// </summary>
public static class NodeStreamKeys
{
    /// <summary>
    /// Gets the stream key for core status events from one node.
    /// </summary>
    public static string CoreStatus(long nodeId) => $"nodes:{nodeId}:core";

    /// <summary>
    /// Gets the stream key for one core install job on one node.
    /// </summary>
    public static string CoreInstall(long nodeId, string jobId) => $"nodes:{nodeId}:install:{jobId}";
}
