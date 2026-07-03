using Contracts.Configurations;

namespace Infrastructure.Services;

/// <summary>
/// Provides cached access to mutable application settings.
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// Gets application settings from memory cache or persistent storage.
    /// </summary>
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates application settings and refreshes the memory cache.
    /// </summary>
    Task<AppSettings> UpdateAsync(
        AppSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates subscription settings and preserves configured webhooks.
    /// </summary>
    Task<AppSettings> UpdateSubscriptionAsync(
        AppSettings subscriptionSettings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a webhook to application settings.
    /// </summary>
    Task<AppWebhook> AddWebhookAsync(
        AppWebhook webhook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a webhook while preserving its secret.
    /// </summary>
    Task<AppWebhook?> UpdateWebhookAsync(
        Guid id,
        AppWebhook webhook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook from application settings.
    /// </summary>
    Task<bool> DeleteWebhookAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
