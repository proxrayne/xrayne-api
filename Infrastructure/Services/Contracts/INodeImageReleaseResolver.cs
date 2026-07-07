using Infrastructure.Dto;

namespace Infrastructure.Services;

/// <summary>
/// Resolves public release assets for Node Docker image archives.
/// </summary>
public interface INodeImageReleaseResolver
{
    Task<NodeImageReleaseAsset> ResolveAsync(CancellationToken cancellationToken);
}
