namespace Api.Responses;

/// <summary>
/// Represents an offset-paginated API response.
/// </summary>
public sealed record PageResponse<T>(
    IReadOnlyList<T> Items,
    int TotalItems,
    int CurrentPage,
    int TotalPages);
