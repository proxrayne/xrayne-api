using XRayne.Infrastructure.States;

namespace XRayne.Infrastructure.Services;

/// <summary>
/// Tracks remote node provisioning states and dispatches installation stream events.
/// </summary>
public interface INodeProvisionStateMachine
{
    NodeProvisionState? GetState(string jobId);

    void Dispatch(string jobId, NodeProvisionState state);
}
