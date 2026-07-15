using Microsoft.Extensions.Logging;
using Quartz;
using Contracts.Enums;
using Contracts.Models;
using Contracts.Utilities;
using Infrastructure.Services;
using Infrastructure.States;

namespace Infrastructure.Tasks;

/// <summary>
/// Runs remote node provisioning in the background.
/// </summary>
public sealed class NodeProvisionJob(
    INodeService nodeService,
    INodeSecretService secretService,
    INodeProvisionStateMachine stateMachine,
    INodeConnectionStateStore connectionStates,
    INodeConnectionManager connectionManager,
    INodeProvisioner provisioner,
    ILogger<NodeProvisionJob> logger) : IJob
{
    public const string NodeIdKey = "nodeId";
    public const string IdentityKey = "jobId";

    public static JobKey GetJobKey(string jobId) => new($"node-provision:{jobId}", "nodes");

    public static TriggerKey GetTriggerKey(string jobId) => new($"node-provision:{jobId}", "nodes");

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var nodeId = context.MergedJobDataMap.GetLong(NodeIdKey);
        var jobId = context.MergedJobDataMap.GetString(IdentityKey) ?? context.FireInstanceId;

        var node = await nodeService.GetByIdAsync(nodeId, ct);
        if (node is null)
        {
            stateMachine.Dispatch(jobId, NodeProvisionState.Failed(nodeId, jobId, "Node was not found."));
            return;
        }

        try
        {
            stateMachine.Dispatch(jobId, NodeProvisionState.Preparing(nodeId, jobId));
            node.InstallationMessage = "Preparing remote node installation.";
            node.Enabled = true;
            connectionStates.Set(new NodeConnectionState(node.Id, NodeConnectionStatus.Connecting, null, null));
            node.LastStatusChange = DateTime.UtcNow;
            await nodeService.UpdateAsync(node, ct);

            var apiKey = secretService.UnprotectApiKey(node.EncryptedApiKey);

            stateMachine.Dispatch(jobId, NodeProvisionState.Installing(nodeId, jobId));
            var result = await provisioner.ProvisionAsync(node, apiKey, jobId, ct);

            node.ConnectedAt = result.VerifiedAt;
            node.LastSeenAt = result.VerifiedAt;
            node.ReconnectAttemptCount = 0;
            node.InstallationMessage = "Remote node is connected.";
            node.Message = null;
            node.LastStatusChange = DateTime.UtcNow;
            await nodeService.UpdateAsync(node, ct);

            stateMachine.Dispatch(jobId, NodeProvisionState.Completed(nodeId, jobId));
            await connectionManager.EnsureConnectedAsync(nodeId, ct);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Remote node provisioning failed for node {NodeId}.", nodeId);

            connectionStates.Set(new NodeConnectionState(node.Id, NodeConnectionStatus.Error, null, null));
            node.Message = exception.Message;
            node.InstallationMessage = exception.Message;
            node.LastStatusChange = DateTime.UtcNow;
            await nodeService.UpdateAsync(node, ct);

            stateMachine.Dispatch(jobId, NodeProvisionState.Failed(nodeId, jobId, exception.Message));
        }
    }
}
