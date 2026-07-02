using Microsoft.Extensions.Logging;
using Quartz;
using XRayne.Contracts.Enums;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.States;

namespace XRayne.Infrastructure.Tasks;

/// <summary>
/// Runs remote node provisioning in the background.
/// </summary>
public sealed class NodeProvisionJob(
    INodeService nodeService,
    INodeSecretService secretService,
    INodeProvisionStateMachine stateMachine,
    IRemoteNodeProvisioner provisioner,
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
            node.Status = NodeStatus.Connecting;
            node.LastStatusChange = DateTime.UtcNow;
            await nodeService.UpdateAsync(node, ct);

            var apiKey = secretService.UnprotectApiKey(node.EncryptedApiKey);

            stateMachine.Dispatch(jobId, NodeProvisionState.Installing(nodeId, jobId));
            var result = await provisioner.ProvisionAsync(node, apiKey, jobId, ct);

            node.Status = NodeStatus.Connected;
            node.ConnectedAt = result.VerifiedAt;
            node.LastSeenAt = result.VerifiedAt;
            node.XrayVersion = result.XrayVersion;
            node.ReconnectAttemptCount = 0;
            node.InstallationMessage = "Remote node is connected.";
            node.Message = null;
            node.LastStatusChange = DateTime.UtcNow;
            await nodeService.UpdateAsync(node, ct);

            stateMachine.Dispatch(jobId, NodeProvisionState.Completed(nodeId, jobId));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Remote node provisioning failed for node {NodeId}.", nodeId);

            node.Status = NodeStatus.Error;
            node.Message = exception.Message;
            node.InstallationMessage = exception.Message;
            node.LastStatusChange = DateTime.UtcNow;
            await nodeService.UpdateAsync(node, ct);

            stateMachine.Dispatch(jobId, NodeProvisionState.Failed(nodeId, jobId, exception.Message));
        }
    }
}
