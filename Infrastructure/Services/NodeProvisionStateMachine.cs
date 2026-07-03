using Microsoft.Extensions.Caching.Memory;
using Infrastructure.States;

namespace Infrastructure.Services;

/// <summary>
/// Stores transient remote node provisioning state and notifies SSE subscribers.
/// </summary>
public sealed class NodeProvisionStateMachine(
    IMemoryCache cache,
    IEventStreamManager eventStreams) : INodeProvisionStateMachine
{
    private static readonly TimeSpan CacheTimeout = TimeSpan.FromHours(6);

    public static string GetStreamKey(string jobId) => $"node-provision:{jobId}";

    public NodeProvisionState? GetState(string jobId)
    {
        return cache.TryGetValue<Dictionary<string, NodeProvisionState>>(nameof(NodeProvisionState), out var values)
            ? values?.GetValueOrDefault(jobId)
            : null;
    }

    public void Dispatch(string jobId, NodeProvisionState state)
    {
        var values = cache.Get<Dictionary<string, NodeProvisionState>>(nameof(NodeProvisionState)) ?? new();
        values[jobId] = state;

        cache.Set(nameof(NodeProvisionState), values, CacheTimeout);
        eventStreams.Dispatch(GetStreamKey(jobId), state);
    }
}
