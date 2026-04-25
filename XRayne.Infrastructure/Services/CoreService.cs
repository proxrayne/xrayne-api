
using Microsoft.Extensions.Logging;
using Xray.Core;

namespace XRayne.Infrastructure.Services;

public class CoreService(ILogger<CoreService> logger, IXrayCore xray) : ICoreService
{
    public async Task StartCore()
    {
        var version = xray.Version();

        logger.LogInformation($"Xray version: {version}");
    }
}