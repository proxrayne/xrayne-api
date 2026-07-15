using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Contracts.Configurations;
using Contracts.Enums;
using Contracts.Models;
using Contracts.Utilities;
using Node.Models;
using Node.Services;
using Data.Entities;
using Infrastructure.Values;
using Node.Grpc;
using Node.Enums;

namespace Infrastructure.Services;

/// <summary>
/// Maintains one live gRPC connection worker per active remote node.
/// </summary>
public sealed class NodeConnectionManager(
    IServiceScopeFactory scopeFactory,
    INodeStreamClientFactory streamClientFactory,
    INodeGrpcChannelProvider grpcChannels,
    INodeConnectionStateStore connectionStates,
    INodeCoreStateStore coreStates,
    INodeLogStore nodeLogs,
    IEventStreamManager eventStreams,
    INodeReconnectPolicy reconnectPolicy,
    IOptions<NodeConnectionOptions> options,
    ILogger<NodeConnectionManager> logger) : INodeConnectionManager, IAsyncDisposable
{
    private const string ConnectedEventType = "connected";
    private const string HeartbeatEventType = "heartbeat";
    private const string CoreStatusEventType = "core_status";
    private const string CoreInstallEventType = "core_install";
    private const string CoreLogEventType = "core_log";

    private readonly ConcurrentDictionary<long, NodeConnectionWorker> workers = new();
    private readonly CancellationTokenSource lifetimeCancellation = new();

    /// <inheritdoc />
    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var nodes = await scope.ServiceProvider.GetRequiredService<INodeService>().GetAllAsync(cancellationToken);
        var runnableNodeIds = nodes
            .Where(CanRun)
            .Select(node => node.Id)
            .ToHashSet();

        foreach (var nodeId in workers.Keys)
        {
            if (!runnableNodeIds.Contains(nodeId))
            {
                await DisconnectAsync(nodeId, cancellationToken);
            }
        }

        foreach (var node in nodes.Where(CanRun))
        {
            StartWorker(node.Id);
        }
    }

    /// <inheritdoc />
    public async Task EnsureConnectedAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        if (node is null || !CanRun(node))
        {
            await DisconnectAsync(nodeId, cancellationToken);
            return;
        }

        StartWorker(nodeId);
    }

    /// <inheritdoc />
    public async Task ReconnectAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        InvalidateChannel(node);
        await StopWorkerAsync(nodeId, cancellationToken);
        connectionStates.Set(new NodeConnectionState(
            nodeId,
            NodeConnectionStatus.Connecting,
            null,
            null));
        await EnsureConnectedAsync(nodeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DisconnectAsync(long nodeId, CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        InvalidateChannel(node);

        if (workers.TryRemove(nodeId, out var worker))
        {
            await worker.StopAsync(cancellationToken);
            await worker.DisposeAsync();
        }

        connectionStates.Set(new NodeConnectionState(
            nodeId,
            ResolveDisconnectedStatus(node),
            null,
            null));
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var worker in workers.Values)
        {
            await worker.StopAsync(CancellationToken.None);
            await worker.DisposeAsync();
        }

        await lifetimeCancellation.CancelAsync();
        lifetimeCancellation.Dispose();
        workers.Clear();
    }

    private void StartWorker(long nodeId)
    {
        workers.AddOrUpdate(
            nodeId,
            _ => NodeConnectionWorker.Start(
                nodeId,
                scopeFactory,
                streamClientFactory,
                connectionStates,
                coreStates,
                nodeLogs,
                eventStreams,
                reconnectPolicy,
                options.Value,
                logger,
                lifetimeCancellation.Token),
            (existingNodeId, existingWorker) =>
            {
                if (!existingWorker.Task.IsCompleted)
                {
                    return existingWorker;
                }

                _ = existingWorker.DisposeAsync();
                return NodeConnectionWorker.Start(
                    existingNodeId,
                    scopeFactory,
                    streamClientFactory,
                    connectionStates,
                    coreStates,
                    nodeLogs,
                    eventStreams,
                    reconnectPolicy,
                    options.Value,
                    logger,
                    lifetimeCancellation.Token);
            });
    }

    private async Task<NodeEntity?> GetNodeAsync(long nodeId, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<INodeService>().GetByIdAsync(nodeId, cancellationToken);
    }

    private bool CanRun(NodeEntity node)
    {
        return node.Enabled
            && HasConnectionConfiguration(node)
            && CanAttemptConnection(node, reconnectPolicy);
    }

    private static bool HasConnectionConfiguration(NodeEntity node)
    {
        return node.ConnectedAt is not null
            && !string.IsNullOrWhiteSpace(node.EncryptedApiKey)
            && !string.IsNullOrWhiteSpace(node.Address)
            && node.ApiPort is > 0 and <= 65535;
    }

    private static bool CanAttemptConnection(NodeEntity node, INodeReconnectPolicy reconnectPolicy)
    {
        return node.ReconnectAttemptCount == 0 || reconnectPolicy.CanRetry(node);
    }

    private async Task StopWorkerAsync(long nodeId, CancellationToken cancellationToken)
    {
        if (!workers.TryRemove(nodeId, out var worker))
        {
            return;
        }

        await worker.StopAsync(cancellationToken);
        await worker.DisposeAsync();
    }

    private void InvalidateChannel(NodeEntity? node)
    {
        if (node is null || string.IsNullOrWhiteSpace(node.Address) || node.ApiPort <= 0)
        {
            return;
        }

        grpcChannels.Invalidate(new NodeEndpoint(node.Id, node.Address, node.ApiPort, string.Empty));
    }

    private static NodeConnectionStatus ResolveDisconnectedStatus(NodeEntity? node)
    {
        if (node?.Enabled == false)
        {
            return NodeConnectionStatus.Disconnected;
        }

        return node?.ReconnectAttemptCount > 0 && !string.IsNullOrWhiteSpace(node.Message)
            ? NodeConnectionStatus.Error
            : NodeConnectionStatus.Disconnected;
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
            INodeStreamClientFactory streamClientFactory,
            INodeConnectionStateStore connectionStates,
            INodeCoreStateStore coreStates,
            INodeLogStore nodeLogs,
            IEventStreamManager eventStreams,
            INodeReconnectPolicy reconnectPolicy,
            NodeConnectionOptions options,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var workerCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = RunAsync(
                nodeId,
                scopeFactory,
                streamClientFactory,
                connectionStates,
                coreStates,
                nodeLogs,
                eventStreams,
                reconnectPolicy,
                options,
                logger,
                workerCancellation.Token);

            return new NodeConnectionWorker(nodeId, workerCancellation, task);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await cancellationTokenSource.CancelAsync();
            try
            {
                await Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync(CancellationToken.None);
            cancellationTokenSource.Dispose();
        }

        private static async Task RunAsync(
            long nodeId,
            IServiceScopeFactory scopeFactory,
            INodeStreamClientFactory streamClientFactory,
            INodeConnectionStateStore connectionStates,
            INodeCoreStateStore coreStates,
            INodeLogStore nodeLogs,
            IEventStreamManager eventStreams,
            INodeReconnectPolicy reconnectPolicy,
            NodeConnectionOptions options,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var node = await GetNodeAsync(scopeFactory, nodeId, cancellationToken);
                if (node is null || !node.Enabled)
                {
                    return;
                }

                if (!HasConnectionConfiguration(node) || !CanAttemptConnection(node, reconnectPolicy))
                {
                    return;
                }

                try
                {
                    await MarkConnectingAsync(scopeFactory, connectionStates, nodeId, "Connecting to remote node stream.", cancellationToken);
                    var endpoint = await CreateEndpointAsync(scopeFactory, node, cancellationToken);
                    var client = streamClientFactory.Create(endpoint);
                    await ConnectStreamAsync(
                        scopeFactory,
                        connectionStates,
                        coreStates,
                        nodeLogs,
                        eventStreams,
                        node,
                        client,
                        options,
                        cancellationToken);
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
                        connectionStates,
                        nodeId,
                        exception.Message,
                        reconnectPolicy,
                        cancellationToken);

                    if (!shouldRetry)
                    {
                        return;
                    }
                }

                await Task.Delay(
                    await GetRetryDelayAsync(reconnectPolicy, options, nodeId, scopeFactory, cancellationToken),
                    cancellationToken);
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

        private static async Task<NodeEndpoint> CreateEndpointAsync(
            IServiceScopeFactory scopeFactory,
            NodeEntity node,
            CancellationToken cancellationToken)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = scopeFactory.CreateScope();
            var secrets = scope.ServiceProvider.GetRequiredService<INodeSecretService>();

            return new NodeEndpoint(
                node.Id,
                node.Address,
                node.ApiPort,
                secrets.UnprotectApiKey(node.EncryptedApiKey));
        }

        private static async Task ConnectStreamAsync(
            IServiceScopeFactory scopeFactory,
            INodeConnectionStateStore connectionStates,
            INodeCoreStateStore coreStates,
            INodeLogStore nodeLogs,
            IEventStreamManager eventStreams,
            NodeEntity node,
            INodeStreamClient client,
            NodeConnectionOptions options,
            CancellationToken cancellationToken)
        {
            var lastPersistedHeartbeat = DateTimeOffset.MinValue;
            var hasPersistedConnection = false;
            var idleTimeout = ResolveIdleTimeout(options);
            await using var enumerator = client.ConnectStreamAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                var moveNextTask = enumerator.MoveNextAsync().AsTask();
                var completedTask = await Task.WhenAny(moveNextTask, Task.Delay(idleTimeout, cancellationToken));
                if (!ReferenceEquals(completedTask, moveNextTask))
                {
                    throw new TimeoutException($"Remote node stream was idle for {idleTimeout.TotalSeconds:N0} seconds.");
                }

                if (!await moveNextTask)
                {
                    break;
                }

                var connectionEvent = enumerator.Current;
                if (connectionEvent.Type is CoreLogEventType)
                {
                    if (connectionEvent.Log is not null)
                    {
                        nodeLogs.Append(node.Id, connectionEvent.Log);
                    }

                    continue;
                }

                if (connectionEvent.Type is CoreStatusEventType)
                {
                    if (connectionEvent.Core is not null)
                    {
                        StoreCoreState(coreStates, node.Id, connectionEvent.Core);
                        eventStreams.Dispatch(NodeStreamKeys.CoreStatus(node.Id), connectionEvent.Core);
                    }

                    continue;
                }

                if (connectionEvent.Type is CoreInstallEventType)
                {
                    if (connectionEvent.Install is not null)
                    {
                        eventStreams.Dispatch(
                            NodeStreamKeys.CoreInstall(node.Id, connectionEvent.Install.JobId),
                            connectionEvent.Install);
                    }

                    if (connectionEvent.Install?.Step is InstallCoreStep.Installed)
                    {
                        await SynchronizeGeoResourcesAsync(
                            scopeFactory,
                            node,
                            cancellationToken);
                    }

                    continue;
                }

                if (connectionEvent.Type is not ConnectedEventType and not HeartbeatEventType
                    || connectionEvent.Ping is null)
                {
                    continue;
                }

                var now = DateTimeOffset.UtcNow;
                MarkHeartbeat(connectionStates, coreStates, node.Id, connectionEvent.Timestamp, connectionEvent.Ping);

                if (!hasPersistedConnection)
                {
                    await MarkConnectedAsync(
                        scopeFactory,
                        node.Id,
                        connectionEvent.Timestamp,
                        cancellationToken);
                    await SynchronizeGeoResourcesAsync(
                        scopeFactory,
                        node,
                        cancellationToken);
                    hasPersistedConnection = true;
                    lastPersistedHeartbeat = now;
                    continue;
                }

                var persistInterval = TimeSpan.FromSeconds(Math.Max(15, options.HeartbeatPersistIntervalSeconds));
                if (lastPersistedHeartbeat == DateTimeOffset.MinValue
                    || now - lastPersistedHeartbeat >= persistInterval)
                {
                    await PersistHeartbeatAsync(
                        scopeFactory,
                        node.Id,
                        connectionEvent.Timestamp,
                        cancellationToken);
                    lastPersistedHeartbeat = now;
                }
            }
        }

        private static async Task MarkConnectingAsync(
            IServiceScopeFactory scopeFactory,
            INodeConnectionStateStore connectionStates,
            long nodeId,
            string message,
            CancellationToken cancellationToken)
        {
            var current = connectionStates.Get(nodeId);
            var snapshot = current is null
                ? new NodeConnectionState(
                    nodeId,
                    NodeConnectionStatus.Connecting,
                    null,
                    null)
                : current with
                {
                    Status = NodeConnectionStatus.Connecting
                };

            connectionStates.Set(snapshot);

            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                node.Message = message;
                node.LastStatusChange = DateTime.UtcNow;
            }, cancellationToken);
        }

        private static void MarkHeartbeat(
            INodeConnectionStateStore connectionStates,
            INodeCoreStateStore coreStates,
            long nodeId,
            DateTimeOffset heartbeatAt,
            PingResponse ping)
        {
            connectionStates.Set(new NodeConnectionState(
                nodeId,
                NodeConnectionStatus.Connected,
                ping.NodeVersion,
                heartbeatAt - ping.Uptime));
            coreStates.Set(new NodeCoreState(
                nodeId,
                ping.Core.IsInstalled,
                ping.Core.IsRunning,
                ping.Core.Version,
                TryMapCoreStatus(ping.Core.Status),
                null,
                null));
        }

        private static async Task MarkConnectedAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            DateTimeOffset heartbeatAt,
            CancellationToken cancellationToken)
        {
            var connectedAt = DateTimeOffset.UtcNow;
            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                node.ConnectedAt = connectedAt;
                node.LastSeenAt = heartbeatAt;
                node.ReconnectAttemptCount = 0;
                node.Message = null;
                node.LastStatusChange = DateTime.UtcNow;
            }, cancellationToken);
        }

        private static async Task SynchronizeGeoResourcesAsync(
            IServiceScopeFactory scopeFactory,
            NodeEntity node,
            CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var geoResources = scope.ServiceProvider.GetRequiredService<INodeGeoResourceService>();

                await geoResources.SynchronizeNodeAsync(
                    node.Admin.Id,
                    node,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
            }
        }

        private static async Task PersistHeartbeatAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            DateTimeOffset heartbeatAt,
            CancellationToken cancellationToken)
        {
            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                node.LastSeenAt = heartbeatAt;
                node.Message = null;
            }, cancellationToken);
        }

        private static async Task<bool> MarkConnectionFailureAsync(
            IServiceScopeFactory scopeFactory,
            INodeConnectionStateStore connectionStates,
            long nodeId,
            string message,
            INodeReconnectPolicy reconnectPolicy,
            CancellationToken cancellationToken)
        {
            var canRetry = false;
            var attempts = 0;
            await UpdateNodeAsync(scopeFactory, nodeId, node =>
            {
                node.ReconnectAttemptCount += 1;
                attempts = node.ReconnectAttemptCount;
                canRetry = reconnectPolicy.CanRetry(node);
                node.Message = message;
                node.LastStatusChange = DateTime.UtcNow;
            }, cancellationToken);

            var current = connectionStates.Get(nodeId);
            var snapshot = current is null
                ? new NodeConnectionState(
                    nodeId,
                    canRetry ? NodeConnectionStatus.Connecting : NodeConnectionStatus.Error,
                    null,
                    null)
                : current with
                {
                    Status = canRetry ? NodeConnectionStatus.Connecting : NodeConnectionStatus.Error
                };

            connectionStates.Set(snapshot);

            return canRetry;
        }

        private static void StoreCoreState(
            INodeCoreStateStore coreStates,
            long nodeId,
            CoreStatusResponse state)
        {
            coreStates.Set(new NodeCoreState(
                nodeId,
                state.IsInstalled,
                state.Status is RemoteCoreStatus.Started,
                state.Version,
                MapCoreStatus(state.Status),
                state.StartedAt,
                state.Uptime));
        }

        private static CoreStatus? TryMapCoreStatus(string? status)
        {
            return Enum.TryParse<CoreStatus>(status, ignoreCase: true, out var result)
                ? result
                : null;
        }

        private static CoreStatus? MapCoreStatus(RemoteCoreStatus? status)
        {
            return status is null
                ? null
                : Enum.Parse<CoreStatus>(status.Value.ToString());
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
            if (node is null || !node.Enabled)
            {
                return;
            }

            update(node);
            await nodes.UpdateAsync(node, cancellationToken);
        }

        private static async Task<TimeSpan> GetRetryDelayAsync(
            INodeReconnectPolicy reconnectPolicy,
            NodeConnectionOptions options,
            long nodeId,
            IServiceScopeFactory scopeFactory,
            CancellationToken cancellationToken)
        {
            var attempt = await GetReconnectAttemptCountAsync(scopeFactory, nodeId, cancellationToken);
            var initialDelay = TimeSpan.FromSeconds(Math.Max(1, options.InitialReconnectDelaySeconds));
            var maxDelay = reconnectPolicy.GetRetryDelay();
            var multiplier = Math.Pow(2, Math.Max(0, attempt - 1));
            var delay = TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * multiplier);

            return delay <= maxDelay ? delay : maxDelay;
        }

        private static TimeSpan ResolveIdleTimeout(NodeConnectionOptions options)
        {
            if (options.StreamIdleTimeoutSeconds > 0)
            {
                return TimeSpan.FromSeconds(options.StreamIdleTimeoutSeconds);
            }

            return TimeSpan.FromSeconds(Math.Max(10, (options.StreamHeartbeatSeconds * 3) + 5));
        }

        private static async Task<int> GetReconnectAttemptCountAsync(
            IServiceScopeFactory scopeFactory,
            long nodeId,
            CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var nodes = scope.ServiceProvider.GetRequiredService<INodeService>();
            var node = await nodes.GetByIdAsync(nodeId, cancellationToken);

            return node?.ReconnectAttemptCount ?? 1;
        }
    }
}
