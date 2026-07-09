using Data.Entities;

namespace Data.Contracts;

/// <summary>
/// Provides persistence for singleton application settings.
/// </summary>
public interface IAppSettingsRepository
{
    /// <summary>
    /// Gets the singleton settings row, creating it with defaults when missing.
    /// </summary>
    Task<AppSettingsEntity> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the singleton settings row.
    /// </summary>
    Task<AppSettingsEntity> UpdateAsync(
        AppSettingsEntity settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a webhook settings row.
    /// </summary>
    Task<AppWebhookEntity> AddWebhookAsync(
        AppWebhookEntity webhook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a webhook settings row without changing its secret.
    /// </summary>
    Task<AppWebhookEntity?> UpdateWebhookAsync(
        Guid id,
        AppWebhookEntity webhook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook settings row.
    /// </summary>
    Task<bool> DeleteWebhookAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
