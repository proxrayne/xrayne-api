using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Services.Contracts;

public interface IDockerComposeFileService
{
    Task WriteApiComposeAsync(
        ProjectPaths paths,
        string imageTag,
        CancellationToken cancellationToken);
}
