namespace Api.Requests;

/// <summary>
/// Defines the complete host order for the current administrator.
/// </summary>
public sealed record UpdateHostOrderRequest
{
    /// <summary>
    /// Gets host identifiers in display order.
    /// </summary>
    public List<long> HostIds { get; init; } = [];
}
