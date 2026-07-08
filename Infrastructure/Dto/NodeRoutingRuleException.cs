namespace Infrastructure.Dto;

/// <summary>
/// Base exception for expected node routing rule operation failures.
/// </summary>
public abstract class NodeRoutingRuleException(string message) : InvalidOperationException(message);
