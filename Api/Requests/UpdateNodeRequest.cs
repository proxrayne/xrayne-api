using System.ComponentModel.DataAnnotations;
namespace Api.Requests;

/// <summary>
/// Request for updating remote node profile metadata.
/// </summary>
public sealed class UpdateNodeRequest
{
    /// <summary>
    /// Gets the human-readable node name shown in the panel.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the operator note for the node.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; init; } = string.Empty;
}
