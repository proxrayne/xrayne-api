using Xray.Core;

namespace XRayne.Core.Services;

public sealed class CoreService : ICoreService
{
    private IXrayCore? _core;

    private IXrayCore _safeCore => _core == null ? throw new Exception("Core not installed.") : _core;

    public bool GetIsRunning() => _core != null && _core.IsStarted();

    public bool GetIsInstalled() => _core != null;

    public string GetVersion() => _safeCore.Version();

    public string? TryGetVersion() => _core == null ? null : _core.Version();
}