using Contracts.Models;
using Contracts.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace Data.Implementations;

/// <summary>
/// Stores live remote node connection states in memory cache.
/// </summary>
public sealed class NodeConnectionStateStore(IMemoryCache cache)
    : CacheStorage<NodeConnectionState, long>(cache, StorageKey, state => state.NodeId),
        INodeConnectionStateStore
{
    private const string StorageKey = "node_connection_states";
}
