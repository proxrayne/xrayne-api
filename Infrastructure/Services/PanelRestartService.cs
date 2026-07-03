using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class PanelRestartService(
    IHostApplicationLifetime lifetime,
    ILogger<PanelRestartService> logger) : IPanelRestartService
{
    private static readonly TimeSpan ShutdownDelay = TimeSpan.FromMilliseconds(500);

    private int _scheduled;

    public bool ScheduleRestart()
    {
        if (Interlocked.CompareExchange(ref _scheduled, 1, 0) != 0)
        {
            logger.LogInformation("Panel restart already scheduled; ignoring duplicate request.");

            return false;
        }

        logger.LogInformation("Panel restart scheduled in {Delay}.", ShutdownDelay);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(ShutdownDelay, lifetime.ApplicationStopping);
            }
            catch (OperationCanceledException)
            {
                // Host уже останавливается по другой причине.
                return;
            }

            logger.LogInformation("Stopping API process for restart.");
            lifetime.StopApplication();
        });

        return true;
    }
}
