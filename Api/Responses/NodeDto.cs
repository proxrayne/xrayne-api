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
