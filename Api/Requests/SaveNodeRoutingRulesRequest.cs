using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests a batch save of node routing rules.
/// </summary>
public sealed class SaveNodeRoutingRulesRequest
{
    /// <summary>
    /// Gets the full ordered snapshot of manually managed routing rules.
    /// </summary>
    [Required]
    public required List<SaveNodeRoutingRuleManualRequest> ManualRules { get; init; }

    /// <summary>
    /// Gets readonly routing rules whose enabled state should be saved.
    /// </summary>
    [Required]
    public required List<SaveNodeRoutingRuleReadonlyRequest> ReadonlyRules { get; init; }
}

/// <summary>
/// Describes one manually managed routing rule in a batch save request.
/// </summary>
public sealed class SaveNodeRoutingRuleManualRequest
{
    /// <summary>
    /// Gets the existing routing rule id, or null when the rule should be created.
    /// </summary>
    public long? Id { get; init; }

    /// <summary>
    /// Gets the xray routing rule JSON configuration.
    /// </summary>
    [Required]
    public required string Config { get; init; }

    /// <summary>
    /// Gets whether the routing rule should be enabled after the save.
    /// </summary>
    public bool Enabled { get; init; }
}

/// <summary>
/// Describes one readonly routing rule enabled-state change in a batch save request.
/// </summary>
public sealed class SaveNodeRoutingRuleReadonlyRequest
{
    /// <summary>
    /// Gets the readonly routing rule id.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets whether the readonly routing rule should be enabled after the save.
    /// </summary>
    public bool Enabled { get; init; }
}
