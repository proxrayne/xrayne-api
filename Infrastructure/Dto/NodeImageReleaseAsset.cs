namespace Infrastructure.Dto;

/// <summary>
/// Describes a public Node Docker image release asset.
/// </summary>
public sealed record NodeImageReleaseAsset(
    string Version,
    string ImageTag,
    string AssetName,
    string DownloadUrl);
