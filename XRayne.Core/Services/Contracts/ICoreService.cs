namespace XRayne.Core.Services;

public interface ICoreService
{
    bool GetIsRunning();
    bool GetIsInstalled();
    string GetVersion();
    string? TryGetVersion();

    Task StopAsync();
    Task SetupAsync(string corePath);
}