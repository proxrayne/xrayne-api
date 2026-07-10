using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Describes a subscription user.
/// </summary>
public sealed record UserDto(
    long Id,
    string Username,
    string Note,
    UserStatus Status,
    DateTimeOffset? ExpireAt,
    DateTimeOffset? OnHoldExpire,
    int ConnectionsCount,
    uint ConnectionLimit,
    ulong TrafficUsedBytes,
    ulong DataLimitBytes,
    LimitResetStrategy? LimitResetStrategy,
    long WarehouseId,
    string WarehouseName);
