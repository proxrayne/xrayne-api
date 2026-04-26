using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Infrastructure.Services;

namespace XRayne.Cli.Commands.Xray;

public sealed class XrayRestartAction(
    ICoreService coreService,
    ICliConsole console,
    ILogger<XrayRestartAction> logger)
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await coreService.RestartCore(cancellationToken);
            console.Success("xray restart completed.");

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "xray restart failed.");
            console.Error(exception.Message);

            return 1;
        }
    }
}
