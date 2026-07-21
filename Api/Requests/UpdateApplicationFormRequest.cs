using System.ComponentModel.DataAnnotations;
using Contracts.Enums;

namespace Api.Requests;

/// <summary>
/// Defines multipart form data required to update a client application profile.
/// </summary>
public sealed class UpdateApplicationFormRequest
{
    /// <summary>
    /// Gets or sets the application display name.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional application website URL.
    /// </summary>
    [MaxLength(64)]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional protocol label used for detection.
    /// </summary>
    [MaxLength(24)]
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the pattern used to detect this application.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string DetectPattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription format produced for this application.
    /// </summary>
    public SubscriptionFormat SubscriptionFormat { get; set; }

    /// <summary>
    /// Gets or sets whether the application is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets application asset references.
    /// </summary>
    public List<string> Assets { get; set; } = [];

    /// <summary>
    /// Gets or sets linked operating system identifiers.
    /// </summary>
    public List<string> OperationSystemIds { get; set; } = [];

    /// <summary>
    /// Gets or sets optional image alternate text.
    /// </summary>
    [MaxLength(64)]
    public string? ImageAlt { get; set; }

    /// <summary>
    /// Gets or sets an optional replacement image file.
    /// </summary>
    public IFormFile? ImageFile { get; set; }
}
