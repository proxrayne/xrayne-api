using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

/// <summary>
/// Stores remote node connection snapshots in the shared memory cache.
/// </summary>
public sealed class RemoteNodeTelemetryCache(IMemoryCache cache) : IRemoteNodeTelemetryCache
{
    /// <inheritdoc />
    public RemoteNodeConnectionSnapshot? Get(long nodeId)
    {
        return cache.Get<RemoteNodeConnectionSnapshot>(GetCacheKey(nodeId));
    }

    /// <inheritdoc />
    public void Set(RemoteNodeConnectionSnapshot snapshot)
    {
        cache.Set(GetCacheKey(snapshot.NodeId), snapshot);
    }

    private static string GetCacheKey(long nodeId) => $"remote_node_connection:{nodeId}";
}
