using Microsoft.EntityFrameworkCore;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories.Panel;

public sealed class PanelSettingsRepository(AppDbContext dbContext) : IPanelSettingsRepository
{
    public async Task<PanelSettings> GetAsync(CancellationToken ct = default)
    {
        var existing = await dbContext.PanelSettings
            .FirstOrDefaultAsync(s => s.Id == PanelSettings.SingletonId, ct);

        if (existing is not null)
        {
            return existing;
        }

        var created = new PanelSettings();
        await dbContext.PanelSettings.AddAsync(created, ct);

        try
        {
            await dbContext.SaveChangesAsync(ct);
            return created;
        }
        catch (DbUpdateException)
        {
            // Параллельный GetAsync уже вставил singleton — перечитываем.
            dbContext.Entry(created).State = EntityState.Detached;
            return await dbContext.PanelSettings
                .FirstAsync(s => s.Id == PanelSettings.SingletonId, ct);
        }
    }

    public async Task UpdateAsync(PanelSettings settings, CancellationToken ct = default)
    {
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.PanelSettings.Update(settings);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task SetPendingRestartAsync(bool pending, CancellationToken ct = default)
    {
        var settings = await GetAsync(ct);
        if (settings.PendingRestart == pending)
        {
            return;
        }

        settings.PendingRestart = pending;
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(ct);
    }
}
