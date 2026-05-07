namespace XRayne.Infrastructure.Services;

public interface IJsonConfigService
{
    void Set<T>(string key, T value);
    void Remove(string key);
    Task SaveAsync(CancellationToken ct = default);
}
