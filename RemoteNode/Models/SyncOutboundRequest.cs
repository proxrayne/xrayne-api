using System.ComponentModel.DataAnnotations;

namespace RemoteNode.Models;

/// <summary>
/// Requests synchronization of a single outbound configuration with the running core.
/// </summary>
public sealed class SyncOutboundRequest
{
    /// <summary>
    /// Gets the outbound configuration as JSON text.
    /// </summary>
    [Required]
    public required string Config { get; init; }
}
