namespace Contracts.Models;

/// <summary>
/// Represents one page of offset-paginated results.
/// </summary>
public sealed record OffsetPage<T>(
    IReadOnlyList<T> Items,
    int TotalItems,
    int CurrentPage,
    int TotalPages);
