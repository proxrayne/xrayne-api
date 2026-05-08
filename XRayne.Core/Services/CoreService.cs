using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xray.Core;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Values;
using XRayne.Repositories.Utilities;

namespace XRayne.Core.Services;

public sealed class CoreService(ILogger<CoreService> logger, IOptions<XrayOptions> options) : ICoreService
{
    private IXrayCore? _core = TryInitializeCore(options.Value, logger);

    private IXrayCore _safeCore => _core == null ? throw new Exception("Core not installed.") : _core;

    public bool GetIsRunning() => _core != null && _core.IsStarted();

    public bool GetIsInstalled() => _core != null;

    public string GetVersion() => _safeCore.Version();

    public string? TryGetVersion() => _core == null ? null : _core.Version();

    public async Task StopAsync()
    {
        if (_core is null || !_core.IsStarted()) return;

        await _core.StopAsync();
    }

    /// <summary>
    /// Setup new core. If core is already running, it will be stopped before setup.
    /// </summary>
    /// <param name="directory">Path to download core folder. Example: /xray-v26_5_3.</param>
    /// <returns></returns>
    public async Task SetupAsync(string directory)
    {
        var newCore = CreateCoreFromDirectory(directory);

        logger.LogInformation($"New core version: {newCore.Version()}.");

        await StopAsync();

        _core = newCore;

        await JsonConfig.SetAsync("Xray:Directory", directory);

        // add start code here later
    }

    private static IXrayCore? TryInitializeCore(XrayOptions options, ILogger logger)
    {
        if (string.IsNullOrEmpty(options.Directory))
        {
            logger.LogInformation("Xray directory is not set. Core will not be initialized.");

            return null;
        }

        return CreateCoreFromDirectory(options.Directory);
    }

    private static IXrayCore CreateCoreFromDirectory(string directory) => new XrayProcessCore(new XrayProcessOptions()
    {
        WorkingDirectory = Path.Combine(PathProvider.Paths.XrayDirectory, directory),
        ProcessName = "xray",
    });
}