namespace XRayne.Cli.Services;

public sealed class ApiInstallationService : IApiInstallationService
{
    private const string EnvPath = "/opt/xrayne/.env";
    private const string ComposePath = "/opt/xrayne/docker-compose.yml";
    private readonly IShellService _shellService;

    public ApiInstallationService(IShellService shellService)
    {
        _shellService = shellService;
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

    public Task<string> RunDockerComposeAsync(
        string arguments,
        CancellationToken cancellationToken)
    {
        EnsureInstalled();

        return _shellService.RunAsync("docker", $"compose {arguments}", InstallDirectory, cancellationToken);
    }

    public async Task<bool> IsApiRunningAsync(CancellationToken cancellationToken)
    {
        var output = await RunDockerComposeAsync("ps --status running --services api", cancellationToken);

        return output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(line => string.Equals(line, "api", StringComparison.OrdinalIgnoreCase));
    }
}
