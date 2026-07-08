namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a node routing rule operation attempted to modify readonly data.
/// </summary>
public sealed class NodeRoutingRuleReadonlyException(string message) : NodeRoutingRuleException(message);
