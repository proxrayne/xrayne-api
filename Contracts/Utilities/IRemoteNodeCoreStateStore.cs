using Contracts.Models;

namespace Contracts.Utilities;

/// <summary>
/// Stores live remote xray-core states in memory.
/// </summary>
public interface IRemoteNodeCoreStateStore : ICacheStorage<RemoteNodeCoreState, long>;
