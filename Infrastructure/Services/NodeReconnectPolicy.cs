using Microsoft.Extensions.Options;
using Contracts.Configurations;
using Data.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Applies configured remote node reconnect limits.
/// </summary>
public sealed class NodeReconnectPolicy(IOptions<NodeConnectionOptions> options) : INodeReconnectPolicy
{
    public bool CanRetry(NodeEntity node)
    {
        return node.Enabled
            && node.ReconnectAttemptCount < Math.Max(0, options.Value.ReconnectAttempts);
    }

    public TimeSpan GetRetryDelay()
    {
        return TimeSpan.FromSeconds(Math.Max(1, options.Value.ReconnectDelaySeconds));
    }
}
