using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests update of a geo resource.
/// </summary>
public sealed class UpdateNodeGeoResourceRequest
{
    /// <summary>
    /// Gets the file name stored on the remote node.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the URL used by auto-updated geo resources.
    /// </summary>
    [MaxLength(2048)]
    public string? Url { get; init; }

    /// <summary>
    /// Gets the update interval used by auto-updated geo resources.
    /// </summary>
    [Range(1, 720)]
    public int? UpdateInterval { get; init; }
}

