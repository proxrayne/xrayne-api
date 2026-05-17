using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XRayne.Infrastructure.Services.PanelSettings;

public sealed class PanelSettingsBootstrapService(
    IPanelSettingsAccessor accessor,
    ILogger<PanelSettingsBootstrapService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await accessor.RefreshFromStoreAsync(cancellationToken);

        if (accessor.PendingRestart)
        {
            logger.LogInformation("Panel settings: clearing PendingRestart after successful start.");
            await accessor.ClearPendingRestartAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
