using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines data required to create a warehouse.
/// </summary>
public sealed record CreateWarehouseRequest
{
    /// <summary>
    /// Gets the warehouse display name.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional warehouse note.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether the warehouse is available for subscriptions.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets inbound identifiers assigned to the warehouse.
    /// </summary>
    public List<long> InboundIds { get; init; } = [];
}
