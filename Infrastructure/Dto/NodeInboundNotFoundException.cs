namespace Infrastructure.Dto;

/// <summary>
/// Represents a missing node or inbound.
/// </summary>
public sealed class NodeInboundNotFoundException(string message) : NodeInboundException(message);
