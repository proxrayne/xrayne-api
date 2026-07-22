namespace Contracts.Models;

/// <summary>
/// Defines filters for searching user connections.
/// </summary>
public sealed record ConnectionFilter
{
    /// <summary>
    /// Gets the text searched in connection names and User-Agent values.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets whether revoked connections should be included.
    /// </summary>
    public bool IncludeRevoked { get; init; }

    /// <summary>
    /// Gets the requested page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int Limit { get; init; } = 10;
}
