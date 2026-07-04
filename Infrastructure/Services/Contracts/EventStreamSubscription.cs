using System.Threading.Channels;

namespace Infrastructure.Services;

public sealed record EventStreamSubscription<T>(Guid Id, string StreamKey, ChannelReader<T> Reader);
