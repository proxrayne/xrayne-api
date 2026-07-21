namespace Api.Requests;

/// <summary>
/// Defines query parameters for listing administrator accounts.
/// </summary>
public sealed record AdminListQuery
{
    /// <summary>
    /// Gets the text searched in usernames and email addresses.
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
