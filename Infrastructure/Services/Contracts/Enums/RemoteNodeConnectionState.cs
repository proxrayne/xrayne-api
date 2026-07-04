namespace Infrastructure.Services;

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
