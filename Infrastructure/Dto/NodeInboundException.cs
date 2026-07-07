namespace Infrastructure.Dto;

/// <summary>
/// Base exception for expected node inbound operation failures.
/// </summary>
public abstract class NodeInboundException(string message) : InvalidOperationException(message);
