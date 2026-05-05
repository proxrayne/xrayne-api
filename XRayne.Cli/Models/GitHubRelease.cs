namespace XRayne.Cli.Models;

public sealed record GitHubRelease(
    string TagName,
    IReadOnlyCollection<GitHubAsset> Assets);
