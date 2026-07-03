using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

public sealed class ChangeAdminPermissionsRequest
{
    [Required]
    public string Permissions { get; set; } = string.Empty;
}
