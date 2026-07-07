using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests an update to a manually managed inbound.
/// </summary>
public sealed class UpdateNodeInboundRequest
{
    /// <summary>
    /// Gets the replacement xray inbound JSON configuration.
    /// </summary>
    [Required]
    public required string Config { get; init; }

    /// <summary>
    /// Gets whether the inbound should be enabled after the update.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
