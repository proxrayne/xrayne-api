using Contracts.Models;

namespace Contracts.Utilities;

/// <summary>
/// Stores live remote node connection states in memory.
/// </summary>
public interface INodeConnectionStateStore : ICacheStorage<NodeConnectionState, long>;
