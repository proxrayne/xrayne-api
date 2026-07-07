namespace Infrastructure.Values;

/// <summary>
/// Provides stream keys for live remote node logs.
/// </summary>
public static class NodeLogStreamKeys
{
    /// <summary>
    /// Gets the stream key for the supplied node.
    /// </summary>
    public static string ForNode(long nodeId) => $"nodes:{nodeId}:logs";
}
