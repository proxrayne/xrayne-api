using Contracts.Exceptions;
using Data.Entities;
using Data.Utilities;
using Microsoft.AspNetCore.Http;

namespace Test.Data;

public sealed class ImagePayloadsTests
{
    private static readonly byte[] PngBytes = [1, 2, 3];

    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpeg")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    public async Task NormalizeForWriteAsync_accepts_supported_content_types(string contentType)
    {
        var payload = await ImagePayloads.NormalizeForWriteAsync(
            CreateFile(PngBytes, contentType),
            required: true,
            CancellationToken.None);

        payload.Should().NotBeNull();
        payload!.Content.Should().Equal(PngBytes);
        payload.ContentType.Should().Be(contentType);
    }

    [Fact]
    public async Task NormalizeForWriteAsync_rejects_svg()
    {
        var act = () => ImagePayloads.NormalizeForWriteAsync(
            CreateFile(PngBytes, "image/svg+xml"),
            required: true,
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task NormalizeForWriteAsync_rejects_empty_file()
    {
        var act = () => ImagePayloads.NormalizeForWriteAsync(
            CreateFile([], "image/png"),
            required: true,
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public void ApplyPayload_does_not_increment_version_for_identical_content()
    {
        var image = CreateImage();

        var changed = ImagePayloads.ApplyPayload(image, new(PngBytes, "image/png"));

        changed.Should().BeFalse();
        image.Version.Should().Be(3);
        image.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void ApplyPayload_increments_version_and_updates_timestamp_when_content_changes()
    {
        var image = CreateImage();

        var changed = ImagePayloads.ApplyPayload(image, new([4, 5, 6], "image/png"));

        changed.Should().BeTrue();
        image.Content.Should().Equal(4, 5, 6);
        image.Version.Should().Be(4);
        image.UpdatedAt.Should().NotBeNull();
    }

    private static FormFile CreateFile(byte[] content, string contentType)
    {
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "imageFile", "image.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static ImageEntity CreateImage()
    {
        return new ImageEntity
        {
            Id = 1,
            Key = "android",
            Alt = "Android",
            Content = PngBytes,
            ContentType = "image/png",
            Version = 3
        };
    }
}
