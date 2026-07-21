using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines multipart form data required to create an operating system option.
/// </summary>
public sealed class CreateOperationSystemFormRequest
{
    /// <summary>
    /// Gets or sets the stable operating system identifier.
    /// </summary>
    [Required]
    [MaxLength(32)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operating system display name.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional note.
    /// </summary>
    [MaxLength(512)]
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets whether the operating system is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets optional image alternate text.
    /// </summary>
    [MaxLength(64)]
    public string? ImageAlt { get; set; }

    /// <summary>
    /// Gets or sets the uploaded image file.
    /// </summary>
    [Required]
    public required IFormFile ImageFile { get; set; }
}
