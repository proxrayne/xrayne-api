using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data.Entities;

[Table("Users")]
[Index(nameof(Username), IsUnique = true)]
public sealed class UserEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    [MaxLength(128)]
    public required string Username { get; set; }

    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    public ulong DataLimit { get; set; }

    public uint DeviceLimit { get; set; } = 1;

    public DateTimeOffset? OnHoldExpire { get; set; }

    public required UserStatus Status { get; set; }

    public LimitResetStrategy? LimitResetStrategy { get; set; }

    public DateTimeOffset? LastTrafficReset { get; set; }

    public DateTimeOffset? OnlineAt { get; set; }

    public DateTimeOffset? ExpireAt { get; set; }

    // relation tables
    public List<ConnectionEntity> Connections { get; set; } = new();

    public WarehouseEntity Warehouse { get; set; } = null!;

    public AdminAccount Admin { get; set; } = null!;
}
