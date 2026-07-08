namespace Infrastructure.Dto;

/// <summary>
/// Indicates that node geo resource input is invalid.
/// </summary>
public sealed class NodeGeoResourceValidationException(string message) : NodeGeoResourceException(message);

