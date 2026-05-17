using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Contracts.Configurations;
using XRayne.Repositories.Panel;
using PanelSettingsEntity = XRayne.Repositories.Entities.PanelSettings;

namespace XRayne.Infrastructure.Services.PanelSettings;

public sealed class PanelSettingsAccessor(
    IServiceScopeFactory scopeFactory,
    ILogger<PanelSettingsAccessor> logger) : IPanelSettingsAccessor
{
    private readonly ConcurrentDictionary<Guid, Action<PanelOptions>> _subscribers = new();
    // Сериализует записи; чтение _current/_pendingRestart без блокировки (volatile).
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private volatile PanelOptions _current = new();
    private volatile bool _pendingRestart;

    public PanelOptions Current => _current;

    public bool PendingRestart => _pendingRestart;

    public IDisposable Subscribe(Action<PanelOptions> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var id = Guid.NewGuid();
        _subscribers[id] = handler;

        return new Subscription(this, id);
    }

    public async Task<SettingsApplyResult> ApplyAsync(PanelOptions next, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(next);

        await _writeLock.WaitAsync(ct);
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPanelSettingsRepository>();

            var entity = await repo.GetAsync(ct);
            var previous = ToOptions(entity);

            var diff = SettingsDiff.Compute(previous, next);
            if (diff.ChangedFields.Count == 0)
            {
                return new SettingsApplyResult(diff.ChangedFields, diff.MaxImpact);
            }

            ApplyOnto(entity, next);
            if (diff.MaxImpact == RestartImpact.FullRestart)
            {
                entity.PendingRestart = true;
            }

            await repo.UpdateAsync(entity, ct);

            var snapshot = next.Clone();
            _current = snapshot;
            _pendingRestart = entity.PendingRestart;

            NotifySubscribers(snapshot);

            return new SettingsApplyResult(diff.ChangedFields, diff.MaxImpact);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task RefreshFromStoreAsync(CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPanelSettingsRepository>();

            var entity = await repo.GetAsync(ct);
            _current = ToOptions(entity);
            _pendingRestart = entity.PendingRestart;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task ClearPendingRestartAsync(CancellationToken ct = default)
    {
        await _writeLock.WaitAsync(ct);
        try
        {
            if (!_pendingRestart)
            {
                return;
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPanelSettingsRepository>();

            await repo.SetPendingRestartAsync(false, ct);
            _pendingRestart = false;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private void NotifySubscribers(PanelOptions snapshot)
    {
        foreach (var (_, handler) in _subscribers)
        {
            try
            {
                handler(snapshot);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Panel settings subscriber threw.");
            }
        }
    }

    private static PanelOptions ToOptions(PanelSettingsEntity entity) => new()
    {
        BindIp = entity.BindIp,
        Domain = entity.Domain,
        Port = entity.Port,
        WebBasePath = entity.WebBasePath,
        SessionLifetimeMinutes = entity.SessionLifetimeMinutes,
        TrustedProxyCidrs = entity.TrustedProxyCidrs,
        CertificatesDirectory = entity.CertificatesDirectory,
        GeoResourcesDirectory = entity.GeoResourcesDirectory,
        PanelCertPublicKeyPath = entity.PanelCertPublicKeyPath,
        PanelCertPrivateKeyPath = entity.PanelCertPrivateKeyPath
    };

    private static void ApplyOnto(PanelSettingsEntity entity, PanelOptions options)
    {
        entity.BindIp = options.BindIp;
        entity.Domain = options.Domain;
        entity.Port = options.Port;
        entity.WebBasePath = options.WebBasePath;
        entity.SessionLifetimeMinutes = options.SessionLifetimeMinutes;
        entity.TrustedProxyCidrs = options.TrustedProxyCidrs;
        entity.CertificatesDirectory = options.CertificatesDirectory;
        entity.GeoResourcesDirectory = options.GeoResourcesDirectory;
        entity.PanelCertPublicKeyPath = options.PanelCertPublicKeyPath;
        entity.PanelCertPrivateKeyPath = options.PanelCertPrivateKeyPath;
    }

    private sealed class Subscription(PanelSettingsAccessor accessor, Guid id) : IDisposable
    {
        public void Dispose() => accessor._subscribers.TryRemove(id, out _);
    }
}
