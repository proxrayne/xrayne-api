using Contracts.Models;
using Contracts.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace Repositories.Implementations;

/// <summary>
/// Stores live remote xray-core states in memory cache.
/// </summary>
public sealed class RemoteNodeCoreStateStore(IMemoryCache cache)
    : CacheStorage<RemoteNodeCoreState, long>(cache, StorageKey, state => state.NodeId),
        IRemoteNodeCoreStateStore
{
    private const string StorageKey = "remote_node_core_states";
}
