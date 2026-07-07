namespace Infrastructure.Dto;

/// <summary>
/// Represents forbidden changes to a readonly inbound.
/// </summary>
public sealed class NodeInboundReadonlyException(string message) : NodeInboundException(message);
