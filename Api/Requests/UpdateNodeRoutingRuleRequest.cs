using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests an update to a manually managed routing rule.
/// </summary>
public sealed class UpdateNodeRoutingRuleRequest
{
    /// <summary>
    /// Gets the replacement xray routing rule JSON configuration.
    /// </summary>
    [Required]
    public required string Config { get; init; }

    /// <summary>
    /// Gets whether the routing rule should be enabled after the update.
    /// </summary>
    public bool Enabled { get; init; }
}
