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
}
