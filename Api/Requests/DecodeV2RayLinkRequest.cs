using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Decodes a v2ray share link into an outbound configuration.
/// </summary>
public class DecodeV2RayLinkRequest
{
    /// <summary>
    /// Gets or sets the v2ray share link.
    /// </summary>
    [Required]
    public required string Link { get; set; }

    /// <summary>
    /// Gets or sets an optional outbound remark override.
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets an optional user email used by supported share formats.
    /// </summary>
    public string? Email { get; set; }
}
