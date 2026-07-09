using Contracts.Enums;

namespace Contracts.Models;

/// <summary>
/// Defines filters for searching users.
/// </summary>
public sealed record UserFilter : CursorQuery
{
    /// <summary>
    /// Gets status values to include.
    /// </summary>
    public IReadOnlyCollection<UserStatus>? Status { get; init; }

    /// <summary>
    /// Gets limit reset strategies to include.
    /// </summary>
    public IReadOnlyCollection<LimitResetStrategy>? LimitResetStrategy { get; init; }
}
