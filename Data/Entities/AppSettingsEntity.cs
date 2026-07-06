using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Models;

namespace Data.Entities;

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
