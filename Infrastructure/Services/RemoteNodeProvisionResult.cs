namespace Infrastructure.Services;

/// <summary>
/// Describes a successful remote node provisioning verification result.
/// </summary>
public sealed record RemoteNodeProvisionResult(DateTimeOffset VerifiedAt);
