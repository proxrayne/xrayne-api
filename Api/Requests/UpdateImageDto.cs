using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines image data used by create and update requests.
/// </summary>
public sealed class UpdateImageDto
{
    /// <summary>
    /// Gets or sets optional image alternate text.
    /// </summary>
    [MaxLength(64)]
    public string? Alt { get; set; }

}
