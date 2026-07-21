using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;
using Xray.Config.Enums;

namespace Data.Entities;

/// <summary>
/// Describes a subscription host entry attached to a managed inbound.
/// </summary>
[Table("Hosts")]
public sealed class HostEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the host identifier.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the host display name.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the host address.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public required string Address { get; set; }

    /// <summary>
    /// Gets or sets an optional TLS server name override.
    /// </summary>
    [MaxLength(128)]
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets an optional HTTP host override.
    /// </summary>
    [MaxLength(128)]
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets an optional request path template.
    /// </summary>
    [MaxLength(256)]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets an optional fragment template.
    /// </summary>
    [MaxLength(100)]
    public string? FragmentTemplate { get; set; }

    /// <summary>
    /// Gets or sets an optional noise template.
    /// </summary>
    [MaxLength(2000)]
    public string? NoiseTemplate { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 country code.
    /// </summary>
    [Required]
    [MinLength(2)]
    [MaxLength(2)]
    public required string CountryAlpha2Code { get; set; }

    /// <summary>
    /// Gets or sets the host ordering position.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets an optional connection port override.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Gets or sets optional ALPN flags stored as a numeric bitmask.
    /// </summary>
    [Column(TypeName = "integer")]
    public ALPN? ALPN { get; set; }

    /// <summary>
    /// Gets or sets the host security mode.
    /// </summary>
    [Column(TypeName = "host_security")]
    public HostSecurity Security { get; set; } = HostSecurity.InboundDefault;

    /// <summary>
    /// Gets or sets the TLS fingerprint.
    /// </summary>
    [Column(TypeName = "fingerprint")]
    public Fingerprint Fingerprint { get; set; }

    /// <summary>
    /// Gets or sets whether the host is available for subscriptions.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether mux is enabled for this host.
    /// </summary>
    public bool IsMuxEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the server name should be reused as the host value.
    /// </summary>
    public bool IsUseServerNameAsHost { get; set; } = false;

    /// <summary>
    /// Gets or sets whether generated user agents should be randomized.
    /// </summary>
    public bool IsRandomUseragent { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this host may increase generated subscription limits.
    /// </summary>
    public bool AllowIncrease { get; set; } = false;

    /// <summary>
    /// Gets or sets the inbound identifier this host belongs to.
    /// </summary>
    public long InboundId { get; set; }

    /// <summary>
    /// Gets or sets the inbound this host belongs to.
    /// </summary>
    [ForeignKey(nameof(InboundId))]
    public InboundEntity Inbound { get; set; } = null!;

    /// <summary>
    /// Gets or sets the administrator identifier that owns the host.
    /// </summary>
    public long AdminId { get; set; }

    /// <summary>
    /// Gets or sets the administrator that owns the host.
    /// </summary>
    [ForeignKey(nameof(AdminId))]
    public AdminAccountEntity Admin { get; set; } = null!;
}
