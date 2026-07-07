namespace Infrastructure.Dto;

/// <summary>
/// Represents invalid inbound input.
/// </summary>
public sealed class NodeInboundValidationException(string message) : NodeInboundException(message);
