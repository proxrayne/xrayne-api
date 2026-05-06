using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Services;

public interface IDockerComposeFileService
{
    Task WriteApiComposeAsync(
        ProjectPaths paths,
        string imageTag,
        CancellationToken cancellationToken);

    Task UseHttpsApiPortAsync(
        string composePath,
        CancellationToken cancellationToken);
}
