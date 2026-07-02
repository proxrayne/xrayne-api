using XRayne.Contracts.Values;

namespace XRayne.Cli.Services.Contracts;

public interface IDockerComposeFileService
{
    Task WriteApiComposeAsync(
        ProjectPaths paths,
        string imageTag,
        CancellationToken cancellationToken);
}
