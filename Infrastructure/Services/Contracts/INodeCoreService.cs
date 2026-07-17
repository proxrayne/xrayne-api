using Data.Entities;

namespace Infrastructure.Services;

public interface INodeCoreService
{
    /// <summary>
    /// Restart xray-core in node if core started.
    /// </summary>
    Task RestartCoreAsync(NodeEntity node, CancellationToken cancellationToken = default);
}