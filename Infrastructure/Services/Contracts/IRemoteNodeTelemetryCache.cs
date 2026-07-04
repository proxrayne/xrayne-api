namespace Infrastructure.Services;

/// <summary>
/// Stores live remote node telemetry snapshots in panel memory.
/// </summary>
public interface IRemoteNodeTelemetryCache
{
    /// <summary>
    /// Gets the latest cached connection snapshot for a remote node.
    /// </summary>
    RemoteNodeConnectionSnapshot? Get(long nodeId);

    /// <summary>
    /// Stores the latest connection snapshot for a remote node.
    /// </summary>
    void Set(RemoteNodeConnectionSnapshot snapshot);
}
