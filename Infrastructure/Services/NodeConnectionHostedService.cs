using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Contracts.Configurations;
using Contracts.Enums;
using Repositories.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Maintains live SSE connections to enabled remote nodes while the panel is running.
/// </summary>
public sealed class NodeConnectionHostedService(
    IServiceScopeFactory scopeFactory,
    INodeReconnectPolicy reconnectPolicy,
    IOptions<NodeConnectionOptions> connectionOptions,
    ILogger<NodeConnectionHostedService> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<long, NodeConnectionWorker> workers = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Remote node connection supervisor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SynchronizeWorkersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception exception)
            {
                logger.LogError(exception, "Remote node connection supervisor synchronization failed.");
            }

            await DelaySupervisorAsync(stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var worker in workers.Values)
        {
            await worker.StopAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task SynchronizeWorkersAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var nodes = await scope.ServiceProvider.GetRequiredService<INodeService>().GetAllAsync(stoppingToken);
        var nodeById = nodes.ToDictionary(node => node.Id);

        foreach (var (nodeId, worker) in workers)
        {
            if (worker.Task.IsCompleted)
            {
                workers.TryRemove(nodeId, out _);
                await worker.DisposeAsync();
                continue;
            }

            if (!nodeById.TryGetValue(nodeId, out var node) || !ShouldRun(node))
            {
                workers.TryRemove(nodeId, out _);
                await worker.StopAsync();
                await worker.DisposeAsync();
            }
        }

        foreach (var node in nodes)
        {
            if (!ShouldRun(node) || workers.ContainsKey(node.Id))
            {
                continue;
            }

            var worker = NodeConnectionWorker.Start(
                node.Id,
                scopeFactory,
                reconnectPolicy,
                connectionOptions.Value,
                logger,
                stoppingToken);

            workers.TryAdd(node.Id, worker);
        }
    }

    private static bool HasConnectionConfiguration(NodeEntity node)
    {
        return node.ConnectedAt is not null
            && !string.IsNullOrWhiteSpace(node.EncryptedApiKey)
            && !string.IsNullOrWhiteSpace(node.Address)
            && node.ApiPort is > 0 and <= 65535;
    }

    private bool ShouldRun(NodeEntity node)
    {
        return node.Status is not NodeStatus.Disabled
            && HasConnectionConfiguration(node)
            && (node.Status is not NodeStatus.Error || reconnectPolicy.CanRetry(node));
    }

    private async Task DelaySupervisorAsync(CancellationToken stoppingToken)
    {
        var seconds = Math.Clamp(connectionOptions.Value.ReconnectDelaySeconds, 5, 60);
        await Task.Delay(TimeSpan.FromSeconds(seconds), stoppingToken);
    }

    private sealed class NodeConnectionWorker : IAsyncDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;

        private NodeConnectionWorker(long nodeId, CancellationTokenSource cancellationTokenSource, Task task)
        {
            NodeId = nodeId;
            this.cancellationTokenSource = cancellationTokenSource;
            Task = task;
        }

        public long NodeId { get; }

        public Task Task { get; }

        public static NodeConnectionWorker Start(
            long nodeId,
            IServiceScopeFactory scopeFactory,
            INodeReconnectPolicy reconnectPolicy,
            NodeConnectionOptions options,
            ILogger logger,
            CancellationToken stoppingToken)
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var task = RunAsync(
                nodeId,
                scopeFactory,
                reconnectPolicy,
                options,
                logger,
                cancellationTokenSource.Token);

            return new NodeConnectionWorker(nodeId, cancellationTokenSource, task);
        }

        public async Task StopAsync()
        {
            await cancellationTokenSource.CancelAsync();
            try
            {
                await Task;
            }
            catch (OperationCanceledException) { }
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            cancellationTokenSource.Dispose();
        }

        private static async Task RunAsync(
            long nodeId,
            IServiceScopeFactory scopeFactory,
            INodeReconnectPolicy reconnectPolicy,
            NodeConnectionOptions options,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var node = await GetNodeAsync(scopeFactory, nodeId, cancellationToken);
                if (node is null || node.Status is NodeStatus.Disabled)
                {
                    return;
                }

                if (!reconnectPolicy.CanRetry(node) && node.Status is NodeStatus.Error)
                {
                    return;
                }

                try
                {
                    await MarkConnectingAsync(scopeFactory, nodeId, "Connecting to remote node stream.", cancellationToken);
                    await ConnectStreamAsync(scopeFactory, node, options, logger, cancellationToken);
                    throw new InvalidOperationException("Remote node stream ended.");
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception, "Remote node {NodeId} connection failed.", nodeId);

                    var shouldRetry = await MarkConnectionFailureAsync(
                        scopeFactory,
                        nodeId,
                        exception.Message,
                        reconnectPolicy,
                        cancellationToken);

                    if (!shouldRetry)
                    {
                        return;
                    }
                }

                await Task.Delay(reconnectPolicy.GetRetryDelay(), cancellationToken);
            }
        }

        private static async Task<NodeEntity?> GetNodeAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();

            return await scope.ServiceProvider.GetRequiredService<INodeService>().GetByIdAsync(nodeId, cancellationToken);
        }

        private static async Task<string> GetApiKeyAsync(
            IServiceScopeFactory scopeFactory,
            NodeEntity node,
            CancellationToken cancellationToken)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = scopeFactory.CreateScope();
            var secrets = scope.ServiceProvider.GetRequiredService<INodeSecretService>();

            return secrets.UnprotectApiKey(node.EncryptedApiKey);
        }

        private static async Task ConnectStreamAsync(
            IServiceScopeFactory scopeFactory,
            NodeEntity node,
            NodeConnectionOptions options,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var apiKey = await GetApiKeyAsync(scopeFactory, node, cancellationToken);
            using var httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan,
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                new UriBuilder("https", node.Address, node.ApiPort, "api/connect").Uri);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
            request.Headers.Add("X-Node-Api-Key", apiKey);

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            await MarkConnectedAsync(scopeFactory, node.Id, cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:", StringComparison.Ordinal))
                {
                    continue;
                }

                var json = line["data:".Length..].Trim();
                var connectionEvent = JsonSerializer.Deserialize<NodeConnectionEvent>(json, JsonOptions);
                if (connectionEvent?.Ping is null)
                {
                    continue;
                }

                await MarkHeartbeatAsync(scopeFactory, node.Id, connectionEvent.Ping, cancellationToken);
            }

            logger.LogInformation("Remote node {NodeId} stream ended.", node.Id);
        }

        private static async Task MarkConnectingAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            string message,
            CancellationToken cancellationToken)
        {
            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                if (node.Status is not NodeStatus.Connected)
                {
                    node.Status = NodeStatus.Connecting;
                    node.Message = message;
                    node.LastStatusChange = DateTime.UtcNow;
                }
            }, cancellationToken);
        }

        private static async Task MarkConnectedAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                node.Status = NodeStatus.Connected;
                node.ConnectedAt = now;
                node.LastSeenAt = now;
                node.ReconnectAttemptCount = 0;
                node.Message = null;
                node.LastStatusChange = DateTime.UtcNow;
            }, cancellationToken);
        }

        private static async Task MarkHeartbeatAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            NodePingResponse ping,
            CancellationToken cancellationToken)
        {
            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                node.Status = NodeStatus.Connected;
                node.LastSeenAt = ping.Timestamp;
                node.XrayVersion = ping.Core.Version;
                node.Message = null;
            }, cancellationToken);
        }

        private static async Task<bool> MarkConnectionFailureAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            string message,
            INodeReconnectPolicy reconnectPolicy,
            CancellationToken cancellationToken)
        {
            var canRetry = false;
            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                node.ReconnectAttemptCount += 1;
                canRetry = reconnectPolicy.CanRetry(node);
                node.Status = canRetry ? NodeStatus.Connecting : NodeStatus.Error;
                node.Message = message;
                node.LastStatusChange = DateTime.UtcNow;
            }, cancellationToken);

            return canRetry;
        }

        private static async Task UpdateNodeAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            Action<NodeEntity> update,
            CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var nodes = scope.ServiceProvider.GetRequiredService<INodeService>();
            var node = await nodes.GetByIdAsync(nodeId, cancellationToken);
            if (node is null || node.Status is NodeStatus.Disabled)
            {
                return;
            }

            update(node);
            await nodes.UpdateAsync(node, cancellationToken);
        }
    }

    private sealed record NodeConnectionEvent(
        string Type,
        DateTimeOffset Timestamp,
        NodePingResponse? Ping);

    private sealed record NodePingResponse(
        string Service,
        string NodeVersion,
        string Environment,
        DateTimeOffset StartedAt,
        DateTimeOffset Timestamp,
        TimeSpan Uptime,
        NodeCoreStatus Core,
        NodeSystemStats System);

    private sealed record NodeCoreStatus(
        bool IsInstalled,
        bool IsRunning,
        string? Version,
        string Status);

    private sealed record NodeSystemStats(
        string MachineName,
        string OSDescription,
        int ProcessorCount,
        long WorkingSetBytes,
        long GcTotalMemoryBytes,
        int CurrentProcessThreadCount,
        long? SystemThreadCount,
        DateTimeOffset StartedAt,
        DateTimeOffset Timestamp,
        TimeSpan Uptime,
        NodeCpuStats Cpu,
        NodeMemoryStats Memory,
        NodeMemoryStats Swap,
        IReadOnlyCollection<NodeVolumeStats> Volumes,
        NodeNetworkStats Network);

    private sealed record NodeCpuStats(
        int LogicalCoreCount,
        double? AverageUsagePercent,
        IReadOnlyCollection<NodeCpuCoreUsage> Cores);

    private sealed record NodeCpuCoreUsage(
        int Index,
        double? UsagePercent);

    private sealed record NodeMemoryStats(
        long TotalBytes,
        long UsedBytes,
        long AvailableBytes);

    private sealed record NodeVolumeStats(
        string Name,
        string FileSystem,
        long TotalBytes,
        long FreeBytes,
        long UsedBytes,
        double UsedPercent);

    private sealed record NodeNetworkStats(
        IReadOnlyCollection<string> IPv4Addresses,
        IReadOnlyCollection<string> IPv6Addresses);
}
