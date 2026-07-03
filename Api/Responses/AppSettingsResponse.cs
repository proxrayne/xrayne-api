using Contracts.Enums;
using Contracts.Models;

namespace Api.Responses;

/// <summary>
/// Contains application settings exposed by the settings API.
/// </summary>
public sealed class AppSettingsResponse
{
    /// <summary>
    /// Gets or sets the subscription profile title.
    /// </summary>
    public string SubscriptionProfileTitle { get; set; } = "XRayne";

    /// <summary>
    /// Gets or sets the optional support URL.
    /// </summary>
    public string? SubscriptionSupportUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional website URL.
    /// </summary>
    public string? SubscriptionWebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the subscription update interval in hours.
    /// </summary>
    public int SubscriptionUpdateIntervalHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the optional subscription announcement.
    /// </summary>
    public SubscriptionAnnounce? Announce { get; set; }

    /// <summary>
    /// Gets or sets configured webhooks.
    /// </summary>
    public required List<AppWebhookDto> Webhooks { get; init; }
}

/// <summary>
/// Contains subscription settings.
/// </summary>
public sealed class AppSubscriptionSettingsDto
{
    /// <summary>
    /// Gets or sets the subscription profile title.
    /// </summary>
    public string SubscriptionProfileTitle { get; set; } = "XRayne";

    /// <summary>
    /// Gets or sets the optional support URL.
    /// </summary>
    public string? SubscriptionSupportUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional website URL.
    /// </summary>
    public string? SubscriptionWebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the subscription update interval in hours.
    /// </summary>
    public int SubscriptionUpdateIntervalHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the optional subscription announcement.
    /// </summary>
    public SubscriptionAnnounce? Announce { get; set; }
}

/// <summary>
/// Contains one webhook without its secret.
/// </summary>
public sealed class AppWebhookDto
{
    /// <summary>
    /// Gets or sets the webhook identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the webhook target URL.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets enabled webhook events.
    /// </summary>
    public WebhookEvent[] Events { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the webhook has a stored secret.
    /// </summary>
    public bool HasSecret { get; set; }

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
    public List<int> SubscriptionExpirationThresholdHours { get; set; } = [];

    /// <summary>
    /// Gets or sets traffic usage thresholds in percents.
    /// </summary>
    public List<int> TrafficThresholdPercents { get; set; } = [];
}
