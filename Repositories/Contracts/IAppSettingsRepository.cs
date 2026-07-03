using Repositories.Entities;

namespace Repositories.Contracts;

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
    Task<AppWebhookSettingsEntity> AddWebhookAsync(
        AppWebhookSettingsEntity webhook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a webhook settings row without changing its secret.
    /// </summary>
    Task<AppWebhookSettingsEntity?> UpdateWebhookAsync(
        Guid id,
        AppWebhookSettingsEntity webhook,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a webhook settings row.
    /// </summary>
    Task<bool> DeleteWebhookAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
