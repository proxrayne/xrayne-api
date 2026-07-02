using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Models;
using XRayne.Repositories.Contracts;
using XRayne.Repositories.Entities;

namespace XRayne.Infrastructure.Services;

/// <summary>
/// Provides memoized access to database-backed application settings.
/// </summary>
public sealed class AppSettingsService(
    IAppSettingsRepository repository,
    IMemoryCache cache,
    IMapper mapper) : IAppSettingsService
{
    private const string CacheKey = "app-settings-singleton";

    public async Task<AppSettings> GetAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKey, out AppSettings? settings) && settings is not null)
        {
            return Clone(settings);
        }

        var entity = await repository.GetAsync(ct);
        settings = Normalize(mapper.Map<AppSettings>(entity));
        cache.Set(CacheKey, settings);

        return Clone(settings);
    }

    public async Task<AppSettings> UpdateAsync(
        AppSettings settings,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var normalized = Normalize(settings);
        var entity = await repository.UpdateAsync(
            mapper.Map<AppSettingsEntity>(normalized),
            ct);

        var updated = Normalize(mapper.Map<AppSettings>(entity));
        cache.Set(CacheKey, updated);

        return Clone(updated);
    }

    public async Task<AppSettings> UpdateSubscriptionAsync(
        AppSettings subscriptionSettings,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(subscriptionSettings);

        var current = await GetAsync(ct);
        current.SubscriptionProfileTitle = subscriptionSettings.SubscriptionProfileTitle;
        current.SubscriptionSupportUrl = subscriptionSettings.SubscriptionSupportUrl;
        current.SubscriptionWebsiteUrl = subscriptionSettings.SubscriptionWebsiteUrl;
        current.SubscriptionUpdateIntervalHours = subscriptionSettings.SubscriptionUpdateIntervalHours;
        current.Announce = subscriptionSettings.Announce;

        return await UpdateAsync(current, ct);
    }

    public async Task<AppWebhook> AddWebhookAsync(
        AppWebhook webhook,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(webhook);

        webhook.Id = webhook.Id == Guid.Empty ? Guid.NewGuid() : webhook.Id;

        var entity = await repository.AddWebhookAsync(
            mapper.Map<AppWebhookSettingsEntity>(Normalize(webhook)),
            ct);
        await RefreshCacheAsync(ct);

        return Normalize(mapper.Map<AppWebhook>(entity));
    }

    public async Task<AppWebhook?> UpdateWebhookAsync(
        Guid id,
        AppWebhook webhook,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(webhook);

        webhook.Id = id;
        webhook.Secret = null;
        var entity = await repository.UpdateWebhookAsync(
            id,
            mapper.Map<AppWebhookSettingsEntity>(Normalize(webhook)),
            ct);
        if (entity is null)
        {
            return null;
        }

        await RefreshCacheAsync(ct);

        return Normalize(mapper.Map<AppWebhook>(entity));
    }

    public async Task<bool> DeleteWebhookAsync(Guid id, CancellationToken ct = default)
    {
        if (!await repository.DeleteWebhookAsync(id, ct))
        {
            return false;
        }

        await RefreshCacheAsync(ct);

        return true;
    }

    private AppSettings Clone(AppSettings settings) =>
        mapper.Map<AppSettings>(settings);

    private async Task RefreshCacheAsync(CancellationToken ct)
    {
        var entity = await repository.GetAsync(ct);
        cache.Set(CacheKey, Normalize(mapper.Map<AppSettings>(entity)));
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        settings.SubscriptionProfileTitle = string.IsNullOrWhiteSpace(settings.SubscriptionProfileTitle)
            ? "XRayne"
            : settings.SubscriptionProfileTitle.Trim();
        settings.SubscriptionSupportUrl = string.IsNullOrWhiteSpace(settings.SubscriptionSupportUrl)
            ? null
            : settings.SubscriptionSupportUrl.Trim();
        settings.SubscriptionWebsiteUrl = string.IsNullOrWhiteSpace(settings.SubscriptionWebsiteUrl)
            ? null
            : settings.SubscriptionWebsiteUrl.Trim();
        settings.SubscriptionUpdateIntervalHours = Math.Max(1, settings.SubscriptionUpdateIntervalHours);
        settings.Announce = Normalize(settings.Announce);
        settings.Webhooks = settings.Webhooks
            .Select(Normalize)
            .ToList();

        return settings;
    }

    private static SubscriptionAnnounce? Normalize(SubscriptionAnnounce? announce)
    {
        if (announce is null)
        {
            return null;
        }

        announce.Message = string.IsNullOrWhiteSpace(announce.Message)
            ? null
            : announce.Message.Trim();
        announce.Url = string.IsNullOrWhiteSpace(announce.Url)
            ? null
            : announce.Url.Trim();

        return announce.Message is null && announce.Url is null
            ? null
            : announce;
    }

    private static AppWebhook Normalize(AppWebhook webhook)
    {
        webhook.Id = webhook.Id == Guid.Empty ? Guid.NewGuid() : webhook.Id;
        webhook.Url = webhook.Url.Trim();
        webhook.Secret = string.IsNullOrWhiteSpace(webhook.Secret) ? null : webhook.Secret;
        webhook.RetryAttempts = Math.Max(0, webhook.RetryAttempts);
        webhook.RetryIntervalSeconds = Math.Max(1, webhook.RetryIntervalSeconds);
        webhook.SubscriptionExpirationThresholdHours = NormalizePositiveThresholds(
            webhook.SubscriptionExpirationThresholdHours);
        webhook.TrafficThresholdPercents = NormalizeTrafficThresholds(webhook.TrafficThresholdPercents);

        return webhook;
    }

    private static List<int> NormalizePositiveThresholds(IEnumerable<int> thresholds) =>
        thresholds
            .Where(value => value > 0)
            .Distinct()
            .Order()
            .ToList();

    private static List<int> NormalizeTrafficThresholds(IEnumerable<int> thresholds) =>
        thresholds
            .Where(value => value is >= 1 and <= 100)
            .Distinct()
            .Order()
            .ToList();
}
