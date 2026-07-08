using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests creation of a routing rule on a remote node.
/// </summary>
public sealed class CreateNodeRoutingRuleRequest
{
    /// <summary>
    /// Gets the display tag for the managed routing rule.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string Tag { get; init; }

    /// <summary>
    /// Gets the xray routing rule JSON configuration.
    /// </summary>
    [Required]
    public required string Config { get; init; }

    /// <summary>
    /// Gets whether the routing rule should be enabled after creation.
    /// </summary>
    public bool Enabled { get; init; }
}
