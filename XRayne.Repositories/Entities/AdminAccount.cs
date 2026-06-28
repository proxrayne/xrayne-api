using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using XRayne.Contracts.Enums;

namespace XRayne.Repositories.Entities;

[Table("Admins")]
[Index(nameof(Username), IsUnique = true)]
public sealed class AdminAccount : CreatedEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(128)]
    public required string Username { get; set; }

    [Required]
    [MaxLength(512)]
    public required string PasswordHash { get; set; }

    public AdminPermission Permissions { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
}
