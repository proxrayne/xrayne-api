using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Describes a user list item.
/// </summary>
public sealed record UserListItemDto(
    long Id,
    string Username,
    UserStatus Status,
    DateTimeOffset? ExpireAt,
    DateTimeOffset? OnHoldExpire,
    int ConnectionsCount,
    uint ConnectionLimit,
    ulong TrafficUsedBytes,
    ulong DataLimitBytes,
    long WarehouseId,
    string WarehouseName);
