using System.Threading.Channels;

namespace Infrastructure.Dto;

/// <summary>
/// Describes an event stream subscription reader.
/// </summary>
public sealed record EventStreamSubscription<T>(Guid Id, string StreamKey, ChannelReader<T> Reader);
