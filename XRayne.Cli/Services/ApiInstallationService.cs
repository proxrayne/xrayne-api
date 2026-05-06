using Microsoft.Extensions.Configuration;

namespace XRayne.Cli.Services;

public sealed class ApiInstallationService : IApiInstallationService
{
    private const string EnvPath = "/opt/xrayne/.env";
    private const string ComposePath = "/opt/xrayne/docker-compose.yml";
    private readonly IShellService _shellService;
    private readonly IConfiguration _configuration;

    public ApiInstallationService(
        IShellService shellService,
        IConfiguration configuration)
    {
        _shellService = shellService;
        _configuration = configuration;
    }

    public string InstallDirectory => "/opt/xrayne";

    public void EnsureInstalled()
    {
        if (!File.Exists(EnvPath))
        {
            throw new InvalidOperationException($"Environment file '{EnvPath}' was not found. Run 'xrayne api install' first.");
        }

        if (!File.Exists(ComposePath))
        {
            throw new InvalidOperationException($"Compose file '{ComposePath}' was not found. Run 'xrayne api install' first.");
        }
    }

    public async Task<string> RunDockerComposeAsync(
        string arguments,
        CancellationToken cancellationToken)
    {
        EnsureInstalled();

        var environment = _configuration
            .AsEnumerable()
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .Where(item => !item.Key.Contains(':', StringComparison.Ordinal))
            .ToDictionary(
                item => item.Key,
                item => item.Value!,
                StringComparer.OrdinalIgnoreCase);
        environment.TryAdd("POSTGRES_HOST_API", "postgres");
        environment.TryAdd("POSTGRES_CONTAINER_PORT", "5432");
        environment.TryAdd("POSTGRES_PORT", "5432");
        environment.TryAdd("API_PORT", "5000");
        environment.TryAdd("PROJECT_PATH", "/opt/xrayne");

        return await _shellService.RunAsync("docker", $"compose {arguments}", InstallDirectory, environment, cancellationToken);
    }

    public async Task<bool> IsApiRunningAsync(CancellationToken cancellationToken)
    {
        var output = await RunDockerComposeAsync("ps --status running --services api", cancellationToken);

        return output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(line => string.Equals(line, "api", StringComparison.OrdinalIgnoreCase));
    }
}
