namespace Infrastructure.Dto;

/// <summary>
/// Base exception for expected node geo resource operation failures.
/// </summary>
public abstract class NodeGeoResourceException(string message) : InvalidOperationException(message);

