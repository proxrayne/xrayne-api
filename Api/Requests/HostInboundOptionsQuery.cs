namespace Api.Requests;

/// <summary>
/// Defines query parameters for host inbound options.
/// </summary>
public sealed record HostInboundOptionsQuery
{
    /// <summary>
    /// Gets text searched in inbound tag and node name.
    /// </summary>
    public string? Search { get; init; }
}
