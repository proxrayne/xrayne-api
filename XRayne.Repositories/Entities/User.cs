using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Xray.Config.Enums;
using XRayne.Contracts.Enums;

namespace XRayne.Repositories.Entities;

[Table("users")]
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

    public List<InboundEntity> Inbounds { get; set; } = new();

    public AdminAccount Admin { get; set; } = null!;
}

public sealed class ClientOption
{
    public Guid? Uuid { get; set; }

    public XtlsFlow? Flow { get; set; }

    public string? Password { get; set; }

    public EncryptionMethod? Method { get; set; }
}