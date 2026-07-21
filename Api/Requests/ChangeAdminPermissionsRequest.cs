using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines fields used to replace administrator permissions.
/// </summary>
public sealed class ChangeAdminPermissionsRequest
{
    /// <summary>
    /// Gets comma-separated administrator permissions.
    /// </summary>
    [Required]
    public string Permissions { get; set; } = string.Empty;
}
