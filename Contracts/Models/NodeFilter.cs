namespace Contracts.Models;

/// <summary>
/// Defines filters for searching remote nodes.
/// </summary>
public sealed record NodeFilter
{
    /// <summary>
    /// Gets the text searched in node names, addresses, and API ports.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets the requested page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int Limit { get; init; } = 10;
}
