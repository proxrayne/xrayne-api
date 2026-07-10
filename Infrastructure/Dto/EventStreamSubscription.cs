using System.Threading.Channels;

namespace Infrastructure.Dto;

/// <summary>
/// Describes an event stream subscription reader.
/// </summary>
public sealed record EventStreamSubscription<T>(
    Guid Id,
    string StreamKey,
    ChannelReader<T> Reader,
    Func<long>? GetDroppedCount = null)
{
    /// <summary>
    /// Gets the number of events dropped because the subscription queue was full.
    /// </summary>
    public long DroppedCount => GetDroppedCount?.Invoke() ?? 0;
}
