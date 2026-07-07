namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a requested node outbound was not found.
/// </summary>
public sealed class NodeOutboundNotFoundException(string message) : NodeOutboundException(message);
