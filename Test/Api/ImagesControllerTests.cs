using Api.Controllers;
using Data.Contracts;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Test.Api;

public sealed class ImagesControllerTests
{
    private readonly IImageRepository _repository = Substitute.For<IImageRepository>();
    private readonly ImagesController _controller;

    public ImagesControllerTests()
    {
        _controller = new ImagesController(_repository)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Get_returns_binary_image_with_immutable_cache_when_version_matches()
    {
        _repository.GetContentByKeyOrDefaultAsync("android", Arg.Any<CancellationToken>())
            .Returns(new ImageContentReadModel([1, 2, 3], "image/png", 7));

        var result = await _controller.Get("android", "7", CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.FileContents.Should().Equal(1, 2, 3);
        file.ContentType.Should().Be("image/png");
        _controller.Response.Headers.CacheControl.ToString()
            .Should().Be("public, max-age=31536000, immutable");
    }

    [Fact]
    public async Task Get_returns_no_cache_when_version_is_missing()
    {
        _repository.GetContentByKeyOrDefaultAsync("android", Arg.Any<CancellationToken>())
            .Returns(new ImageContentReadModel([1, 2, 3], "image/png", 7));

        await _controller.Get("android", null, CancellationToken.None);

        _controller.Response.Headers.CacheControl.ToString().Should().Be("no-cache");
    }
}
