using Microsoft.Extensions.Options;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Enums;
using XRayne.Repositories.Entities;

namespace XRayne.Infrastructure.Services;

/// <summary>
/// Applies configured remote node reconnect limits.
/// </summary>
public sealed class NodeReconnectPolicy(IOptions<NodeConnectionOptions> options) : INodeReconnectPolicy
{
    public bool CanRetry(NodeEntity node)
    {
        return node.Status is not NodeStatus.Disabled
            && node.ReconnectAttemptCount < Math.Max(0, options.Value.ReconnectAttempts);
    }

    public TimeSpan GetRetryDelay()
    {
        return TimeSpan.FromSeconds(Math.Max(1, options.Value.ReconnectDelaySeconds));
    }
}
