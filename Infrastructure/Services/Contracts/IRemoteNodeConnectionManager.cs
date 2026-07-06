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

}
