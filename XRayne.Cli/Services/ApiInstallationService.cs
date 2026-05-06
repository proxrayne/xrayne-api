using Microsoft.Extensions.Configuration;
using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Services;

public sealed class ApiInstallationService : IApiInstallationService
{
    private readonly IShellService _shellService;
    private readonly IConfiguration _configuration;

    public ApiInstallationService(
        IShellService shellService,
        IConfiguration configuration)
    {
        _shellService = shellService;
        _configuration = configuration;
    }

    public string InstallDirectory => PathProvider.Paths.Root;

    public void EnsureInstalled()
    {
        if (!File.Exists(PathProvider.Paths.EnvConfig))
        {
            throw new InvalidOperationException($"Environment file '{PathProvider.Paths.EnvConfig}' was not found. Run 'xrayne api install' first.");
        }

        if (!File.Exists(PathProvider.Paths.DockerCompose))
        {
            throw new InvalidOperationException($"Compose file '{PathProvider.Paths.DockerCompose}' was not found. Run 'xrayne api install' first.");
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
        environment.TryAdd("PROJECT_PATH", PathProvider.Paths.Root);

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
