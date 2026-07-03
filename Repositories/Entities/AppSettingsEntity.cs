using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Models;

namespace Repositories.Entities;

/// <summary>
/// Stores singleton application settings.
/// </summary>
[Table("AppSettings")]
public sealed class AppSettingsEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets the fixed singleton row identifier.
    /// </summary>
    public const int SingletonId = 1;

    /// <summary>
    /// Gets or sets the fixed singleton row identifier.
    /// </summary>
    [Key]
    public int Id { get; set; } = SingletonId;

    /// <summary>
    /// Gets or sets the subscription profile title.
    /// </summary>
    [MaxLength(256)]
    public string SubscriptionProfileTitle { get; set; } = "XRayne";

    /// <summary>
    /// Gets or sets the optional subscription support URL.
    /// </summary>
    [MaxLength(2048)]
    public string? SubscriptionSupportUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional subscription website URL.
    /// </summary>
    [MaxLength(2048)]
    public string? SubscriptionWebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the subscription update interval in hours.
    /// </summary>
    public int SubscriptionUpdateIntervalHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the optional subscription announcement.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public SubscriptionAnnounce? Announce { get; set; }

    /// <summary>
    /// Gets or sets configured webhooks.
    /// </summary>
    public List<AppWebhookSettingsEntity> Webhooks { get; set; } = [];
}


/// <summary>
/// Stores one application settings webhook.
/// </summary>
[Table("AppWebhooks")]
public sealed class AppWebhookSettingsEntity : CreateUpdateEntity
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
    [MaxLength(2048)]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the enabled event bit mask.
    /// </summary>
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
