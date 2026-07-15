using Contracts.Configurations;
using Infrastructure.Values;
using Microsoft.Extensions.Options;
using Node.Models;

namespace Infrastructure.Services;

/// <summary>
/// Stores bounded live xray-core logs per remote node.
/// </summary>
public sealed class NodeLogStore(
    IEventStreamManager eventStreams,
    IOptions<NodeLogOptions> options) : INodeLogStore
{
    private readonly Lock gate = new();
    private readonly Dictionary<long, Queue<RemoteLogEntry>> entries = new();

    /// <inheritdoc />
    public int NormalizeLimit(int? limit)
    {
        var maxEntries = GetMaxEntries();
        if (limit is null)
        {
            return Math.Clamp(options.Value.DefaultLimit, 1, maxEntries);
        }

        return limit <= 0
            ? maxEntries
            : Math.Clamp(limit.Value, 1, maxEntries);
    }

    /// <inheritdoc />
    public IReadOnlyList<RemoteLogEntry> Get(long nodeId, int? limit = null)
    {
        var normalizedLimit = NormalizeLimit(limit);
        lock (gate)
        {
            return entries.TryGetValue(nodeId, out var sourceEntries)
                ? sourceEntries.Reverse().Take(normalizedLimit).Reverse().ToArray()
                : [];
        }
    }

    /// <inheritdoc />
    public void Append(long nodeId, RemoteLogStreamEvent logEvent)
    {
        if (logEvent.Entry is not null)
        {
            AppendEntry(nodeId, logEvent.Entry, logEvent);
        }

        if (logEvent.Entries is not null)
        {
            foreach (var entry in logEvent.Entries)
            {
                AppendEntry(nodeId, entry, logEvent);
            }
        }
    }

    /// <inheritdoc />
    public void Remove(long nodeId)
    {
        lock (gate)
        {
            entries.Remove(nodeId);
        }
    }

    private void AppendEntry(long nodeId, RemoteLogEntry entry, RemoteLogStreamEvent sourceEvent)
    {
        lock (gate)
        {
            if (!entries.TryGetValue(nodeId, out var sourceEntries))
            {
                sourceEntries = new Queue<RemoteLogEntry>();
                entries[nodeId] = sourceEntries;
            }

            sourceEntries.Enqueue(entry);
            while (sourceEntries.Count > GetMaxEntries())
            {
                _ = sourceEntries.Dequeue();
            }
        }

        eventStreams.Dispatch(
            NodeLogStreamKeys.ForNode(nodeId),
            new RemoteLogStreamEvent(
                "entry",
                null,
                entry,
                sourceEvent.Sequence,
                sourceEvent.DroppedCount,
                sourceEvent.Source));
    }

    private int GetMaxEntries() => Math.Max(1, options.Value.MaxEntriesPerSource);
}
