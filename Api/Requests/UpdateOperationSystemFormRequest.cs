using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines multipart form data required to update an operating system option.
/// </summary>
public sealed class UpdateOperationSystemFormRequest
{
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
    public string Note { get; set; } = string.Empty;

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
    /// Gets or sets an optional replacement image file.
    /// </summary>
    public IFormFile? ImageFile { get; set; }
}
