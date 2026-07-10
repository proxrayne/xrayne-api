using Contracts.Enums;

namespace Api.Requests;

/// <summary>
/// Defines query parameters for listing users.
/// </summary>
public sealed record UserListQuery
{
    /// <summary>
    /// Gets the text searched in usernames.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets status values to include.
    /// </summary>
    public List<UserStatus> Statuses { get; init; } = [];

    /// <summary>
    /// Gets the field used to order users.
    /// </summary>
    public UserSortBy SortBy { get; init; } = UserSortBy.CreatedAt;

    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public SortOrder SortOrder { get; init; } = SortOrder.Desc;

    /// <summary>
    /// Gets the requested page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int Limit { get; init; } = 10;
}
