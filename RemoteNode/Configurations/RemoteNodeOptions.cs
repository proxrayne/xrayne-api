namespace RemoteNode.Configurations;

/// <summary>
/// Configures remote node API protocol client behavior.
/// </summary>
public sealed class RemoteNodeOptions
{
    /// <summary>
    /// Gets or sets the timeout in seconds for non-stream remote node requests.
    /// </summary>
    public int PingTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the HTTP/2 keepalive ping delay for active gRPC calls.
    /// </summary>
    public int KeepAlivePingDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the HTTP/2 keepalive ping timeout for active gRPC calls.
    /// </summary>
    public int KeepAlivePingTimeoutSeconds { get; set; } = 20;
}
