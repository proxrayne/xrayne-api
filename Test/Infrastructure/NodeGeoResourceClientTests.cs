using Grpc.Core;
using Node.Services;
using Node.Values;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Test.Infrastructure;

/// <summary>
/// Tests remote node geo resource client behavior.
/// </summary>
public sealed class NodeGeoResourceClientTests
{
    [Fact]
    public async Task WriteGeoResourceUploadChunksAsync_SplitsContentInto512KbChunks()
    {
        var content = Enumerable.Range(0, GeoResourceTransferDefaults.ChunkSizeBytes + 1)
            .Select(value => (byte)(value % byte.MaxValue))
            .ToArray();
        await using var stream = new MemoryStream(content);
        var writer = new TestClientStreamWriter<Proto.UploadGeoResourceChunk>();

        await NodeGeoResourceClient.WriteGeoResourceUploadChunksAsync(
            "geoip.dat",
            stream,
            writer,
            CancellationToken.None);

        writer.Messages.Should().HaveCount(2);
        writer.Messages[0].FileName.Should().Be("geoip.dat");
        writer.Messages[0].Content.Length.Should().Be(GeoResourceTransferDefaults.ChunkSizeBytes);
        writer.Messages[1].FileName.Should().Be("geoip.dat");
        writer.Messages[1].Content.Length.Should().Be(1);
    }

    private sealed class TestClientStreamWriter<T> : IClientStreamWriter<T>
    {
        public List<T> Messages { get; } = [];

        public WriteOptions? WriteOptions { get; set; }

        public Task WriteAsync(T message)
        {
            Messages.Add(message);

            return Task.CompletedTask;
        }

        public Task CompleteAsync()
        {
            return Task.CompletedTask;
        }
    }
}
