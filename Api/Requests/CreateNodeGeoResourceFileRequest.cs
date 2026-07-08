using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Api.Requests;

/// <summary>
/// Requests creation of a static geo resource from an uploaded file.
/// </summary>
public sealed class CreateNodeGeoResourceFileRequest
{
    /// <summary>
    /// Gets the uploaded geo resource file.
    /// </summary>
    [Required]
    public required IFormFile File { get; init; }

    /// <summary>
    /// Gets an optional file name override.
    /// </summary>
    [MaxLength(128)]
    public string? FileName { get; init; }
}

