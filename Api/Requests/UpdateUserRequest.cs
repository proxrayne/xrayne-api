using Contracts.Enums;

namespace Api.Requests;

/// <summary>
/// Defines data required to update a subscription user.
/// </summary>
public sealed record UpdateUserRequest
{
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
    /// Gets whether the user should be disabled.
    /// </summary>
    public bool Disabled { get; init; }
}
