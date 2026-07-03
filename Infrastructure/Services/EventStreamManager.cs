using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Infrastructure.Services;

public sealed class EventStreamManager : IEventStreamManager
{
    private readonly ConcurrentDictionary<Guid, IEventStreamSubscriptionState> subscriptions = new();

    public EventStreamSubscription<object> Subscribe(string streamKey)
    {
        return Subscribe<object>(streamKey);
    }

    public EventStreamSubscription<T> Subscribe<T>(string streamKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamKey);

        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });

        var state = new EventStreamSubscriptionState<T>(streamKey, channel);
        subscriptions[id] = state;

        return new EventStreamSubscription<T>(id, streamKey, channel.Reader);
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
        public string StreamKey { get; } = streamKey;

        public void Dispatch(object? data)
        {
            if (data is null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
                {
                    return;
                }

                channel.Writer.TryWrite(default!);
                return;
            }

            if (data is T typedData)
            {
                channel.Writer.TryWrite(typedData);
            }
        }

        public void Complete()
        {
            channel.Writer.TryComplete();
        }
    }
}

public sealed record EventStreamSubscription<T>(Guid Id, string StreamKey, ChannelReader<T> Reader);
