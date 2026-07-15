using Contracts.Models;
using Contracts.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace Data.Implementations;

/// <summary>
/// Stores live remote xray-core states in memory cache.
/// </summary>
public sealed class NodeCoreStateStore(IMemoryCache cache)
    : CacheStorage<NodeCoreState, long>(cache, StorageKey, state => state.NodeId),
        INodeCoreStateStore
{
    private const string StorageKey = "remote_node_core_states";
}
