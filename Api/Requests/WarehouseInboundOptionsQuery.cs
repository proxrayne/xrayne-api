namespace Api.Requests;

/// <summary>
/// Defines query parameters for warehouse inbound option lookup.
/// </summary>
public sealed record WarehouseInboundOptionsQuery
{
    /// <summary>
    /// Gets the text searched in inbound tags or node names.
    /// </summary>
    public string? Search { get; init; }
}
