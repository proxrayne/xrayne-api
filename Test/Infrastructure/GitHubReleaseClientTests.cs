using Infrastructure.Services;

namespace Test.Infrastructure;

public sealed class GitHubReleaseClientTests
{
    [Theory]
    [InlineData("proxrayne/xrayne-api", "proxrayne/xrayne-api")]
    [InlineData("https://github.com/proxrayne/xrayne-api", "proxrayne/xrayne-api")]
    [InlineData("https://github.com/proxrayne/xrayne-api/", "proxrayne/xrayne-api")]
    public void NormalizeRepositoryFullNameSupportsFullNameAndGitHubUrl(string value, string expected)
    {
        var result = GitHubReleaseClient.NormalizeRepositoryFullName(value);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://example.com/proxrayne/xrayne-api")]
    [InlineData("proxrayne")]
    [InlineData("proxrayne/xrayne-api/extra")]
    public void NormalizeRepositoryFullNameRejectsUnsupportedRepositoryValues(string value)
    {
        var action = () => GitHubReleaseClient.NormalizeRepositoryFullName(value);

        action.Should().Throw<ArgumentException>();
    }
}
