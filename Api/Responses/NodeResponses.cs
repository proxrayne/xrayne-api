using Contracts.Enums;
using Infrastructure.States;

namespace Api.Responses;

/// <summary>
/// Remote node response model.
/// </summary>
public sealed record NodeDto(
    long Id,
    string Name,
    string Address,
    int Port,
    int ApiPort,
    string SSHUsername,
    string WorkingDirectory,
    string Note,
    CertificateMode CertificateMode,
    string ApiKeyFingerprint,
    string? XrayVersion,
    DateTime LastStatusChange,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? ConnectedAt,
    int ReconnectAttemptCount,
    NodeStatus Status,
    SSHAuthType AuthType,
    string? Message,
    string InstallationMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>
/// Response returned after scheduling remote node creation.
/// </summary>
public sealed record CreateNodeResponse(NodeDto Node, string JobId);

/// <summary>
/// Response returned when scheduling or applying a node operation.
/// </summary>
public sealed record NodeOperationResponse(NodeDto Node, string Status);

/// <summary>
/// Remote node provisioning state response.
/// </summary>
public sealed record NodeProvisionStateResponse(NodeProvisionState State);
