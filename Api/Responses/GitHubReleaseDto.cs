namespace Api.Responses;

public sealed class GitHubReleaseDto
{
    public long Id { get; init; }
    public required string TagName { get; init; }
    public required string HtmlUrl { get; init; }
    public bool Prerelease { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset PublishedAt { get; init; }
}