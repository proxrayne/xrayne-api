using Infrastructure.States;

namespace Api.Responses;

/// <summary>
/// Remote node provisioning state response.
/// </summary>
public sealed record NodeProvisionStateResponse(NodeProvisionState State);
