using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests an update to a manually managed outbound.
/// </summary>
public sealed class UpdateNodeOutboundRequest
{
    /// <summary>
    /// Gets the replacement xray outbound JSON configuration.
    /// </summary>
    [Required]
    public required string Config { get; init; }

    /// <summary>
    /// Gets whether the outbound should be enabled after the update.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
