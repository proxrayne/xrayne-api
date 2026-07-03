using System.ComponentModel.DataAnnotations;
using Contracts.Values;

namespace Api.Requests;

public sealed class CreateAdminRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;

    public string Permissions { get; set; } = string.Join(",", [AdminPermissionNames.CreateUsers, AdminPermissionNames.EditUsers]);
}
