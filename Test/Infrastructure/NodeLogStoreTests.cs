using Contracts.Configurations;
using Infrastructure.Services;
using Infrastructure.Values;
using Microsoft.Extensions.Options;
using RemoteNode.Models;

namespace Test.Infrastructure;

public sealed class NodeLogStoreTests
{
    [Fact]
    public void Get_ReturnsEntriesForRequestedNodeOnly()
    {
        var store = CreateStore();

        store.Append(1, CreateEvent("node one"));
        store.Append(2, CreateEvent("node two"));

        var result = store.Get(1, 10);

        result.Should().ContainSingle();
        result[0].Message.Should().Be("node one");
    }

    [Fact]
    public void Append_TrimsOldestEntriesWhenBufferIsFull()
    {
        var store = CreateStore(maxEntries: 2);

        store.Append(1, CreateEvent("first"));
        store.Append(1, CreateEvent("second"));
        store.Append(1, CreateEvent("third"));

        var result = store.Get(1, 10);

        result.Select(entry => entry.Message).Should().Equal("second", "third");
    }

    [Fact]
    public async Task Append_DispatchesEntryToNodeLogStream()
    {
        var eventStreams = new EventStreamManager();
        var store = CreateStore(eventStreams: eventStreams);
        var subscription = eventStreams.Subscribe<RemoteLogStreamEvent>(
            NodeLogStreamKeys.ForNode(1));

        try
        {
            store.Append(1, CreateEvent("streamed"));

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var logEvent = await subscription.Reader.ReadAsync(cts.Token);

            logEvent.Entry.Should().NotBeNull();
            logEvent.Entry!.Message.Should().Be("streamed");
        }
        finally
        {
            eventStreams.Unsubscribe(subscription.Id);
        }
    }

    private static NodeLogStore CreateStore(int maxEntries = 5000, EventStreamManager? eventStreams = null)
    {
        return new NodeLogStore(
            eventStreams ?? new EventStreamManager(),
            Options.Create(new NodeLogOptions
            {
                DefaultLimit = 500,
                MaxEntriesPerSource = maxEntries
            }));
    }

    private static RemoteLogStreamEvent CreateEvent(string message)
    {
        var entry = new RemoteLogEntry(
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            "information",
            message);

        return new RemoteLogStreamEvent("entry", null, entry);
    }
}
