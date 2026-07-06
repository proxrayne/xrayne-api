using Contracts.Enums;

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
    DateTime LastStatusChange,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? ConnectedAt,
    int ReconnectAttemptCount,
    bool Enabled,
    NodeConnectionStatus Status,
    SSHAuthType AuthType,
    string? Message,
    string InstallationMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
