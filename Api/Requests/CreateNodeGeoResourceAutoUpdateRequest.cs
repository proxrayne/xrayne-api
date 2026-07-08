using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Requests creation of an auto-updated geo resource.
/// </summary>
public sealed class CreateNodeGeoResourceAutoUpdateRequest
{
    /// <summary>
    /// Gets the file name stored on the remote node.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the URL used to download the geo resource.
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public required string Url { get; init; }

    /// <summary>
    /// Gets the five-field Unix cron template.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public required string CronTemplate { get; init; }
}

