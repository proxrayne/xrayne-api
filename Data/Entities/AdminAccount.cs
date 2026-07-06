using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Contracts.Enums;

namespace Data.Entities;

[Table("Admins")]
[Index(nameof(Username), IsUnique = true)]
public sealed class AdminAccount : CreatedEntity
{
    /// <summary>
    /// Gets or sets the administrator account identifier.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique administrator username.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the optional administrator email address used for certificate issuance.
    /// </summary>
    [MaxLength(320)]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the administrator password hash.
    /// </summary>
    [Required]
    [MaxLength(512)]
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the administrator permissions.
    /// </summary>
    public AdminPermission Permissions { get; set; }

    /// <summary>
    /// Gets or sets the last successful login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }
}
