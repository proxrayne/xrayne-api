using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Xray.Config.Enums;
using Contracts.Enums;

namespace Data.Entities;

[Table("Users")]
[Index(nameof(Username), IsUnique = true)]
public sealed class User : CreateUpdateEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public required string Username { get; set; }

    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    public ulong DataLimit { get; set; }

    public DateTimeOffset? OnHoldExpire { get; set; }

    public required UserStatus Status { get; set; }

    public LimitResetStrategy? LimitResetStrategy { get; set; }

    [Column(TypeName = "jsonb")]
    public Dictionary<Protocol, ClientOption> Options { get; set; } = new();

    public DateTimeOffset? LastTrafficReset { get; set; }

    public DateTimeOffset? OnlineAt { get; set; }

    public DateTimeOffset? ExpireAt { get; set; }

    // relation tables
    public List<InboundEntity> Inbounds { get; set; } = new();

    public AdminAccount Admin { get; set; } = null!;
}
