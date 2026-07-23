namespace Contracts.Configurations;

/// <summary>
/// Configures remote node connection and reconnect behavior.
/// </summary>
public sealed class NodeConnectionOptions
{
    public int ReconnectAttempts { get; set; } = 3;

    public int InitialReconnectDelaySeconds { get; set; } = 1;

    public int ReconnectDelaySeconds { get; set; } = 30;

    public int PingTimeoutSeconds { get; set; } = 10;

    public int StreamHeartbeatSeconds { get; set; } = 15;

    public int StreamIdleTimeoutSeconds { get; set; } = 0;

    public int StreamChannelCapacity { get; set; } = 256;

    public int LogStreamBatchSize { get; set; } = 50;

    public int LogStreamBatchWindowMilliseconds { get; set; } = 250;

    public int GrpcKeepAliveDelaySeconds { get; set; } = 60;

    public int GrpcKeepAliveTimeoutSeconds { get; set; } = 20;

    public int HeartbeatPersistIntervalSeconds { get; set; } = 60;

    public int SupervisorIntervalSeconds { get; set; } = 60;
}
