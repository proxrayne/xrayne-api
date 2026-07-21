using System.ComponentModel.DataAnnotations;
using Contracts.Values;

namespace Api.Requests;

/// <summary>
/// Defines fields used to create an administrator account.
/// </summary>
public sealed class CreateAdminRequest
{
    /// <summary>
    /// Gets the administrator username.
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets the administrator email address.
    /// </summary>
    [EmailAddress]
    [MaxLength(320)]
    public string? Email { get; set; }

    /// <summary>
    /// Gets the administrator password.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets comma-separated administrator permissions.
    /// </summary>
    public string Permissions { get; set; } = string.Join(",", [AdminPermissionNames.CreateUsers, AdminPermissionNames.EditUsers]);
}
