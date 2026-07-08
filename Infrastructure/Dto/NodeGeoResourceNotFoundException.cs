namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a requested node geo resource was not found.
/// </summary>
public sealed class NodeGeoResourceNotFoundException(string message) : NodeGeoResourceException(message);

