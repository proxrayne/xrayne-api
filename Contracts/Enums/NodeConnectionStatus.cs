namespace Contracts.Enums;

/// <summary>
/// Describes the current live remote node connection status.
/// </summary>
public enum NodeConnectionStatus
{
    /// <summary>
    /// The panel is connecting or reconnecting to the remote node.
    /// </summary>
    Connecting,

    /// <summary>
    /// The remote node stream is connected.
    /// </summary>
    Connected,

    /// <summary>
    /// The remote node stream failed.
    /// </summary>
    Error,

    /// <summary>
    /// The remote node stream is intentionally disconnected.
    /// </summary>
    Disconnected,

    /// <summary>
    /// The remote node is disabled by configuration.
    /// </summary>
    Disabled
}
