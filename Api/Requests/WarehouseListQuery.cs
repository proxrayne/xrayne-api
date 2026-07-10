namespace Api.Requests;

/// <summary>
/// Defines query parameters for listing warehouses.
/// </summary>
public sealed record WarehouseListQuery
{
    /// <summary>
    /// Gets the text searched in warehouse names.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets enabled state to include.
    /// </summary>
    public bool? Enabled { get; init; }

    /// <summary>
    /// Gets inbound identifiers where any match includes the warehouse.
    /// </summary>
    public List<long> InboundIds { get; init; } = [];

    /// <summary>
    /// Gets the requested page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int Limit { get; init; } = 10;
}
