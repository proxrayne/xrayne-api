namespace XRayne.Infrastructure.Services;

public interface ICoreService
{
    Task StartCore(CancellationToken cancellationToken = default);

    Task StopCore(CancellationToken cancellationToken = default);

    Task RestartCore(CancellationToken cancellationToken = default);
}
