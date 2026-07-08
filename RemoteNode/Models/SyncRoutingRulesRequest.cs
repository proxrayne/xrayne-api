using System.ComponentModel.DataAnnotations;

namespace RemoteNode.Models;

/// <summary>
/// Requests synchronization of ordered routing rules with the running core.
/// </summary>
public sealed class SyncRoutingRulesRequest
{
    /// <summary>
    /// Gets the enabled routing rules as JSON array text.
    /// </summary>
    [Required]
    public required string Rules { get; init; }
}
