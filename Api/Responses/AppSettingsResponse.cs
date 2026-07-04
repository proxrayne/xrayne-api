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
