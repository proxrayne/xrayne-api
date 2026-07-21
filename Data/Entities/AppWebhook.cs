using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

/// <summary>
/// Stores one application settings webhook.
/// </summary>
[Table("AppWebhooks")]
public sealed class AppWebhookEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the webhook identifier.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning application settings identifier.
    /// </summary>
    public int AppSettingsId { get; set; } = AppSettingsEntity.SingletonId;

    /// <summary>
    /// Gets or sets the owning application settings row.
    /// </summary>
    public AppSettingsEntity AppSettings { get; set; } = null!;

    /// <summary>
    /// Gets or sets the webhook target URL.
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the enabled event bit mask.
    /// </summary>
    [Column(TypeName = "numeric(20,0)")]
    public ulong Events { get; set; }

    /// <summary>
    /// Gets or sets the optional webhook secret.
    /// </summary>
    [MaxLength(1024)]
    public string? Secret { get; set; }

    /// <summary>
    /// Gets or sets retry attempts count after a delivery error.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets retry interval in seconds.
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets subscription expiration thresholds in hours.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<int> SubscriptionExpirationThresholdHours { get; set; } = [];

    /// <summary>
    /// Gets or sets traffic usage thresholds in percents.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<int> TrafficThresholdPercents { get; set; } = [];
}
