namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a readonly node outbound was edited through a manual operation.
/// </summary>
public sealed class NodeOutboundReadonlyException(string message) : NodeOutboundException(message);
