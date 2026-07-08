namespace Infrastructure.Dto;

/// <summary>
/// Indicates that a node geo resource conflicts with another geo resource.
/// </summary>
public sealed class NodeGeoResourceConflictException(string message) : NodeGeoResourceException(message);

