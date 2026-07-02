using System.ComponentModel.DataAnnotations;

namespace XRayne.Api.Requests;

public sealed class ChangeAdminPasswordRequest
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
