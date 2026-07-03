using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using Repositories.Entities;

namespace Repositories.Implementations;

/// <summary>
/// Persists singleton application settings in PostgreSQL.
/// </summary>
public sealed class AppSettingsRepository(AppDbContext dbContext) : IAppSettingsRepository
{
    public async Task<AppSettingsEntity> GetAsync(CancellationToken ct = default)
    {
        var settings = await dbContext.AppSettings
            .Include(item => item.Webhooks)
            .SingleOrDefaultAsync(item => item.Id == AppSettingsEntity.SingletonId, ct);
        if (settings is not null)
        {
            return settings;
        }

        settings = new AppSettingsEntity();

        await dbContext.AppSettings.AddAsync(settings, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(settings).ReloadAsync(ct);

        return settings;
    }

    public async Task<AppSettingsEntity> UpdateAsync(
        AppSettingsEntity settings,
        CancellationToken ct = default)
    {
        var current = await GetAsync(ct);

        current.SubscriptionProfileTitle = settings.SubscriptionProfileTitle;
        current.SubscriptionSupportUrl = settings.SubscriptionSupportUrl;
        current.SubscriptionWebsiteUrl = settings.SubscriptionWebsiteUrl;
        current.SubscriptionUpdateIntervalHours = settings.SubscriptionUpdateIntervalHours;
        current.Announce = settings.Announce;
        current.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(current).Collection(item => item.Webhooks).LoadAsync(ct);

        return current;
    }

    public async Task<AppWebhookSettingsEntity> AddWebhookAsync(
        AppWebhookSettingsEntity webhook,
        CancellationToken ct = default)
    {
        await GetAsync(ct);

        webhook.Id = webhook.Id == Guid.Empty ? Guid.NewGuid() : webhook.Id;
        webhook.AppSettingsId = AppSettingsEntity.SingletonId;

        await dbContext.AppWebhookSettings.AddAsync(webhook, ct);
        await dbContext.SaveChangesAsync(ct);

        return webhook;
    }

    public async Task<AppWebhookSettingsEntity?> UpdateWebhookAsync(
        Guid id,
        AppWebhookSettingsEntity webhook,
        CancellationToken ct = default)
    {
        var current = await dbContext.AppWebhookSettings
            .SingleOrDefaultAsync(item => item.Id == id, ct);
        if (current is null)
        {
            return null;
        }

        current.Url = webhook.Url;
        current.Events = webhook.Events;
        current.RetryAttempts = webhook.RetryAttempts;
        current.RetryIntervalSeconds = webhook.RetryIntervalSeconds;
        current.SubscriptionExpirationThresholdHours = webhook.SubscriptionExpirationThresholdHours;
        current.TrafficThresholdPercents = webhook.TrafficThresholdPercents;
        current.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return current;
    }

    public async Task<bool> DeleteWebhookAsync(Guid id, CancellationToken ct = default)
    {
        var current = await dbContext.AppWebhookSettings
            .SingleOrDefaultAsync(item => item.Id == id, ct);
        if (current is null)
        {
            return false;
        }

        dbContext.AppWebhookSettings.Remove(current);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
