using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

public class DecodeV2RayLinkRequest
{
    [Required]
    public required string Link { get; set; }

    public string? Remark { get; set; }

    public string? Email { get; set; }
}
