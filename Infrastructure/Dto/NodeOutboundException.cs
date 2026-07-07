namespace Infrastructure.Dto;

/// <summary>
/// Base exception for expected node outbound operation failures.
/// </summary>
public abstract class NodeOutboundException(string message) : InvalidOperationException(message);
