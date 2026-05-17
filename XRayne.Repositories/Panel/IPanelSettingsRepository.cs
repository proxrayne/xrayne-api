using XRayne.Repositories.Entities;

namespace XRayne.Repositories.Panel;

public interface IPanelSettingsRepository
{
    Task<PanelSettings> GetAsync(CancellationToken ct = default);

    Task UpdateAsync(PanelSettings settings, CancellationToken ct = default);

    Task SetPendingRestartAsync(bool pending, CancellationToken ct = default);
}
