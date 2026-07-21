using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data.Entities;

/// <summary>
/// Stores a subscription user and its account limits.
/// </summary>
[Table("Users")]
[Index(nameof(Username), IsUnique = true)]
public sealed class UserEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the unique username.
    /// </summary>
    [MaxLength(128)]
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the optional user note.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the traffic limit in bytes.
    /// </summary>
    [Column(TypeName = "numeric(20,0)")]
    public ulong DataLimit { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of user connections.
    /// </summary>
    public uint ConnectionLimit { get; set; } = 1;

    /// <summary>
    /// Gets or sets when an on-hold user should expire.
    /// </summary>
    public DateTimeOffset? OnHoldExpire { get; set; }

    /// <summary>
    /// Gets or sets the current user status.
    /// </summary>
    [Column(TypeName = "user_status")]
    public required UserStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the traffic limit reset strategy.
    /// </summary>
    [Column(TypeName = "limit_reset_strategy")]
    public LimitResetStrategy? LimitResetStrategy { get; set; }

    /// <summary>
    /// Gets or sets when user traffic was last reset.
    /// </summary>
    public DateTimeOffset? LastTrafficReset { get; set; }

    /// <summary>
    /// Gets or sets when the user was last observed online.
    /// </summary>
    public DateTimeOffset? OnlineAt { get; set; }

    /// <summary>
    /// Gets or sets when the user expires.
    /// </summary>
    public DateTimeOffset? ExpireAt { get; set; }

    /// <summary>
    /// Gets or sets connection credentials assigned to this user.
    /// </summary>
    public List<ConnectionEntity> Connections { get; set; } = [];

    /// <summary>
    /// Gets or sets the warehouse identifier that defines this user's available inbounds.
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// Gets or sets the warehouse that defines this user's available inbounds.
    /// </summary>
    public WarehouseEntity Warehouse { get; set; } = null!;

    /// <summary>
    /// Gets or sets the administrator identifier that owns the user.
    /// </summary>
    public long AdminId { get; set; }

    /// <summary>
    /// Gets or sets the administrator that owns the user.
    /// </summary>
    public AdminAccountEntity Admin { get; set; } = null!;
}
