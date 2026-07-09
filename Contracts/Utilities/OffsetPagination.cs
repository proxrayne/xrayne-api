namespace Contracts.Utilities;

/// <summary>
/// Provides helpers for page and limit normalization.
/// </summary>
public static class OffsetPagination
{
    /// <summary>
    /// Default number of items returned by offset-paginated endpoints.
    /// </summary>
    public const int DefaultLimit = 10;

    /// <summary>
    /// Maximum number of items returned by offset-paginated endpoints.
    /// </summary>
    public const int MaxLimit = 100;

    /// <summary>
    /// Normalizes a requested page number.
    /// </summary>
    public static int NormalizePage(int page)
    {
        return page <= 0 ? 1 : page;
    }

    /// <summary>
    /// Normalizes a requested page size.
    /// </summary>
    public static int NormalizeLimit(int limit)
    {
        if (limit <= 0)
        {
            return DefaultLimit;
        }

        return Math.Min(limit, MaxLimit);
    }

    /// <summary>
    /// Calculates total page count for a total item count and normalized limit.
    /// </summary>
    public static int CalculateTotalPages(int totalItems, int limit)
    {
        if (totalItems <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(totalItems / (double)limit);
    }
}
