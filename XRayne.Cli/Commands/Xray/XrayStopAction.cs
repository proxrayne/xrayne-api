using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Infrastructure.Services;

namespace XRayne.Cli.Commands.Xray;

public sealed class XrayStopAction(
    ICoreService coreService,
    ICliConsole console,
    ILogger<XrayStopAction> logger)
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await coreService.StopCore(cancellationToken);
            console.Success("xray stop completed.");

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "xray stop failed.");
            console.Error(exception.Message);

            return 1;
        }
    }
}
