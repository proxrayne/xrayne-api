namespace XRayne.Infrastructure.Services;

public interface IEnvConfigService
{
    string? Get(string key);

    void Set(string key, string value);

    void Remove(string key);

    Task SaveAsync(CancellationToken cancellationToken = default);
}
