using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests creation of an outbound on a remote node.
/// </summary>
public sealed class CreateNodeOutboundRequest
{
    /// <summary>
    /// Gets the xray outbound JSON configuration.
    /// </summary>
    [Required]
    public required string Config { get; init; }

    /// <summary>
    /// Gets whether the outbound should be enabled after creation.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
