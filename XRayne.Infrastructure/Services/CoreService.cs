using Microsoft.Extensions.Logging;
using Xray.Core;

namespace XRayne.Infrastructure.Services;

public class CoreService(ILogger<CoreService> logger, IXrayCore xray) : ICoreService
{
    public Task StartCore(CancellationToken cancellationToken = default)
    {
        var version = xray.Version();

        logger.LogInformation("Starting Xray core. Version: {Version}", version);

        return Task.CompletedTask;
    }

    public Task StopCore(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Stopping Xray core.");

        return Task.CompletedTask;
    }

    public async Task RestartCore(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Restarting Xray core.");

        await StopCore(cancellationToken);
        await StartCore(cancellationToken);
    }
}
