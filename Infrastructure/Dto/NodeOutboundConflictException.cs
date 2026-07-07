namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a node outbound conflicts with another node outbound.
/// </summary>
public sealed class NodeOutboundConflictException(string message) : NodeOutboundException(message);
