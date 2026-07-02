using XRayne.Repositories.Entities;

namespace XRayne.Infrastructure.Services;

/// <summary>
/// Decides whether the panel may retry connecting to a remote node.
/// </summary>
public interface INodeReconnectPolicy
{
    bool CanRetry(NodeEntity node);

    TimeSpan GetRetryDelay();
}
