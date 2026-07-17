using Contracts.Utilities;
using Data.Entities;
using Node.Models;
using Node.Services;

namespace Infrastructure.Services;

public sealed class NodeCoreService(
    INodeCoreClientFactory coreClientFactory,
    INodeCoreStateStore coreStateStore,
    INodeCoreConfigBuilder coreConfigBuilder,
    INodeSecretService secrets) : INodeCoreService
{
    /// <inheritdoc/>
    public async Task RestartCoreAsync(NodeEntity node, CancellationToken ct = default)
    {
        if (!coreStateStore.TryGet(node.Id, out var state) || state?.IsRunning != true) return;


        await CreateCoreClient(node).RestartCoreAsync(coreConfigBuilder.Build(node), ct);
    }

    private INodeCoreClient CreateCoreClient(NodeEntity node) => coreClientFactory.Create(new NodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey)));
}