namespace Api.Requests;

/// <summary>
/// Defines query parameters for listing remote nodes.
/// </summary>
public sealed record NodeListQuery
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
