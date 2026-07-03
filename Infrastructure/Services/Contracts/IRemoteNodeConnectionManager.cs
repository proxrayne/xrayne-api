using RemoteNode.Models;

namespace Infrastructure.Services;

/// <summary>
/// Manages live remote node stream connections while the panel is running.
/// </summary>
public interface IRemoteNodeConnectionManager
{
    /// <summary>
    /// Starts live stream workers for every runnable saved node.
    /// </summary>
    Task StartAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that a live stream worker exists for the specified node.
    /// </summary>
    Task EnsureConnectedAsync(long nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets node reconnect state and starts a live stream worker immediately.
    /// </summary>
    Task ReconnectAsync(long nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the live stream worker for the specified node.
    /// </summary>
    Task DisconnectAsync(long nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest in-memory live connection snapshot for a node.
    /// </summary>
    RemoteNodeConnectionSnapshot? GetSnapshot(long nodeId);
}

/// <summary>
/// Describes a live remote node connection snapshot held in panel memory.
/// </summary>
public sealed record RemoteNodeConnectionSnapshot(
    long NodeId,
    RemoteNodeConnectionState State,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ConnectedAt,
    DateTimeOffset? LastHeartbeatAt,
    int ReconnectAttemptCount,
    string? Message,
    NodePingResponse? Telemetry);

/// <summary>
/// Describes the current in-memory remote node connection state.
/// </summary>
public enum RemoteNodeConnectionState
{
    /// <summary>
    /// The node worker is connecting or reconnecting.
    /// </summary>
    Connecting,

    /// <summary>
    /// The node stream is connected.
    /// </summary>
    Connected,

    /// <summary>
    /// The node stream failed and no more automatic retries are available.
    /// </summary>
    Error,

    /// <summary>
    /// The node worker was intentionally stopped.
    /// </summary>
    Disconnected
}
