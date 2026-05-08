namespace XRayne.Core.Services;

public interface ICoreService
{
    public bool GetIsRunning();
    public bool GetIsInstalled();
    public string GetVersion();
    public string? TryGetVersion();
}