using Contracts.Enums;
using Xray.Config.Enums;

namespace Api.Responses;

/// <summary>
/// Describes a user connection without exposing credentials.
/// </summary>
public sealed record ConnectionDto(
    long Id,
    long UserId,
    string? Name,
    XtlsFlow Flow,
    EncryptionMethod Method,
    DeviceVerificationMethod DeviceVerificationMethod,
    bool IsConnected,
    bool Revoked,
    DateTimeOffset? OnlineAt,
    DateTimeOffset? ConnectedAt,
    DateTimeOffset? SubscriptionUpdatedAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
