using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests a new ordering for manually managed node routing rules.
/// </summary>
public sealed class UpdateNodeRoutingRuleOrderRequest
{
    /// <summary>
    /// Gets manual routing rule ids in their desired order.
    /// </summary>
    [Required]
    public required List<long> RoutingRuleIds { get; init; }
}
