using XRayne.Contracts.Configurations;
using XRayne.Contracts.Models;

namespace XRayne.Infrastructure.Services;

public interface ISettingsService
{
    PanelSettings Current { get; }
    bool PendingRestart { get; }

    Task<SettingsUpdateState> ApplyAsync(PanelSettings next, CancellationToken ct = default);
}