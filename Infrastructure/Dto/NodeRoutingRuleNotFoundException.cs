namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a node routing rule operation referenced a missing resource.
/// </summary>
public sealed class NodeRoutingRuleNotFoundException(string message) : NodeRoutingRuleException(message);
