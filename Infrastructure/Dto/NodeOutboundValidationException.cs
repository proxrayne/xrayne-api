namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a node outbound configuration is invalid.
/// </summary>
public sealed class NodeOutboundValidationException(string message) : NodeOutboundException(message);
