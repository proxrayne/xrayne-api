using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines fields used to change an administrator password.
/// </summary>
public sealed class ChangeAdminPasswordRequest
{
    /// <summary>
    /// Gets the old password required for non-super administrators.
    /// </summary>
    public string? OldPassword { get; set; }

    /// <summary>
    /// Gets the new password.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets the new password confirmation required for non-super administrators.
    /// </summary>
    public string? PasswordConfirmation { get; set; }
}
