using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests creation of an inbound on a remote node.
/// </summary>
public sealed class CreateNodeInboundRequest
{
    /// <summary>
    /// Gets the xray inbound JSON configuration.
    /// </summary>
    [Required]
    public required string Config { get; init; }

    /// <summary>
    /// Gets whether the inbound should be enabled after creation.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
