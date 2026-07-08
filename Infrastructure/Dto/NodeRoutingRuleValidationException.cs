namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a node routing rule request is invalid.
/// </summary>
public sealed class NodeRoutingRuleValidationException(string message) : NodeRoutingRuleException(message);
