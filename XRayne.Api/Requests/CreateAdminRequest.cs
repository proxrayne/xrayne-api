using System.ComponentModel.DataAnnotations;
using XRayne.Contracts.Values;

namespace XRayne.Api.Requests;

public sealed class CreateAdminRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string Permissions { get; set; } = string.Join(",", [AdminPermissionNames.CreateUsers, AdminPermissionNames.EditUsers]);
}
