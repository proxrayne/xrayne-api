using Microsoft.AspNetCore.Mvc;

namespace Api.Requests;

/// <summary>
/// Defines query parameters for listing user connections.
/// </summary>
public sealed record ConnectionListQuery
{
    /// <summary>
    /// Gets the text searched in connection names and User-Agent values.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets whether revoked connections should be included.
    /// </summary>
    [FromQuery(Name = "include_revoked")]
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
