using Contracts.Enums;

namespace Api.Requests;

/// <summary>
/// Defines data required to create a subscription user.
/// </summary>
public sealed record CreateUserRequest
{
    /// <summary>
    /// Gets the unique username.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional user note.
    /// </summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>
    /// Gets the configured traffic limit in bytes.
    /// </summary>
    public ulong DataLimitBytes { get; init; }

    /// <summary>
    /// Gets the reset strategy used when an expiration date is configured.
    /// </summary>
    public LimitResetStrategy? LimitResetStrategy { get; init; }

    /// <summary>
    /// Gets the maximum number of connections available to the user.
    /// </summary>
    public uint ConnectionLimit { get; init; } = 1;

    /// <summary>
    /// Gets when the user expires.
    /// </summary>
    public DateTimeOffset? ExpireAt { get; init; }

    /// <summary>
    /// Gets the warehouse identifier assigned to the user.
    /// </summary>
    public long WarehouseId { get; init; }

    /// <summary>
    /// Gets the optional on-hold duration in days.
    /// </summary>
    public int? OnHoldDays { get; init; }
}
