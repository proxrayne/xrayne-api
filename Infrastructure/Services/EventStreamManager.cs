using System.Collections.Concurrent;
using System.Threading.Channels;
using Contracts.Configurations;
using Infrastructure.Dto;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class EventStreamManager : IEventStreamManager
{
    private const int DefaultChannelCapacity = 256;

    private readonly ConcurrentDictionary<Guid, IEventStreamSubscriptionState> subscriptions = new();
    private readonly int channelCapacity;

    public EventStreamManager(IOptions<NodeConnectionOptions>? options = null)
    {
        channelCapacity = NormalizeCapacity(options?.Value.StreamChannelCapacity ?? DefaultChannelCapacity);
    }

    public EventStreamSubscription<object> Subscribe(string streamKey)
    {
        return Subscribe<object>(streamKey);
    }

    public EventStreamSubscription<T> Subscribe<T>(string streamKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamKey);

        var id = Guid.NewGuid();
        var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(channelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait,
            AllowSynchronousContinuations = false,
        });

        var state = new EventStreamSubscriptionState<T>(streamKey, channel);
        subscriptions[id] = state;

        return new EventStreamSubscription<T>(id, streamKey, channel.Reader, state.GetDroppedCount);
    }

    public bool Unsubscribe(Guid subscriptionId)
    {
        if (!subscriptions.TryRemove(subscriptionId, out var subscription))
        {
            return false;
        }

        subscription.Complete();

        return true;
    }

    public void Dispatch(string streamKey, object? data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamKey);

        foreach (var subscription in subscriptions.Values)
        {
            if (string.Equals(subscription.StreamKey, streamKey, StringComparison.Ordinal))
            {
                subscription.Dispatch(data);
            }
        }
    }

    public void Dispatch<T>(string streamKey, T data)
    {
        Dispatch(streamKey, (object?)data);
    }

    private interface IEventStreamSubscriptionState
    {
        string StreamKey { get; }
        void Dispatch(object? data);
        void Complete();
    }

    private sealed class EventStreamSubscriptionState<T>(
        string streamKey,
        Channel<T> channel) : IEventStreamSubscriptionState
    {
        private long droppedCount;

        public string StreamKey { get; } = streamKey;

        public void Dispatch(object? data)
        {
            if (data is null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
                {
                    return;
                }

                if (!channel.Writer.TryWrite(default!))
                {
                    Interlocked.Increment(ref droppedCount);
                }

                return;
            }

            if (data is T typedData)
            {
                if (!channel.Writer.TryWrite(typedData))
                {
                    Interlocked.Increment(ref droppedCount);
                }
            }
        }

        public long GetDroppedCount()
        {
            return Interlocked.Read(ref droppedCount);
        }

        public void Complete()
        {
            channel.Writer.TryComplete();
        }
    }

    private static int NormalizeCapacity(int value)
    {
        return Math.Clamp(value, 1, 100_000);
    }
}
