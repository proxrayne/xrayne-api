namespace Node.Configurations;

/// <summary>
/// Configures remote node API protocol client behavior.
/// </summary>
public sealed class NodeOptions
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

    /// <summary>
    /// Gets or sets the maximum gRPC message size in bytes for node calls.
    /// </summary>
    public int MaxMessageSizeBytes { get; set; } = 128 * 1024 * 1024;
}
