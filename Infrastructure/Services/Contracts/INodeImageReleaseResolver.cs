namespace XRayne.Infrastructure.Services;

/// <summary>
/// Resolves public release assets for XRayne.Node Docker image archives.
/// </summary>
public interface INodeImageReleaseResolver
{
    Task<NodeImageReleaseAsset> ResolveAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Describes a public XRayne.Node Docker image release asset.
/// </summary>
public sealed record NodeImageReleaseAsset(
    string Version,
    string ImageTag,
    string AssetName,
    string DownloadUrl);
