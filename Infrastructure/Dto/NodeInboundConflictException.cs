namespace Infrastructure.Dto;

/// <summary>
/// Represents inbound uniqueness conflicts within a node.
/// </summary>
public sealed class NodeInboundConflictException(string message) : NodeInboundException(message);
