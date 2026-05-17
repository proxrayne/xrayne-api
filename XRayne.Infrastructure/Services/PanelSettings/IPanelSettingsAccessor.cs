using XRayne.Contracts.Configurations;

namespace XRayne.Infrastructure.Services.PanelSettings;

public interface IPanelSettingsAccessor
{
    PanelOptions Current { get; }

    bool PendingRestart { get; }

    // Handler вызывается синхронно из ApplyAsync — обязан быть быстрым и без I/O,
    // иначе блокирует HTTP-ответ. Dispose возвращаемого токена снимает подписку.
    IDisposable Subscribe(Action<PanelOptions> handler);

    Task<SettingsApplyResult> ApplyAsync(PanelOptions next, CancellationToken ct = default);

    Task RefreshFromStoreAsync(CancellationToken ct = default);

    Task ClearPendingRestartAsync(CancellationToken ct = default);
}

public sealed record SettingsApplyResult(IReadOnlyList<string> ChangedFields, RestartImpact MaxImpact)
{
    public bool RequiresRestart => MaxImpact == RestartImpact.FullRestart;

    public IReadOnlyList<string> HotReloadedFields => MaxImpact == RestartImpact.FullRestart
        ? Array.Empty<string>()
        : ChangedFields;
}
