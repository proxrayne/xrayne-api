using Data.Contracts;
using Data.Entities;

namespace Infrastructure.Services;

public sealed class NodeService(INodeRepository repository) : INodeService
{
    public Task<List<NodeEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<List<NodeEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default)
        => repository.GetAllAsync(adminId, cancellationToken);

    public Task<NodeEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, cancellationToken);

    public Task<NodeEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(adminId, id, cancellationToken);

    public Task<NodeEntity> AddAsync(NodeEntity node, CancellationToken cancellationToken = default)
        => repository.AddAsync(node, cancellationToken);

    public Task<NodeEntity> AddAsync(Guid adminId, NodeEntity node, CancellationToken cancellationToken = default)
        => repository.AddAsync(adminId, node, cancellationToken);

    public Task<NodeEntity?> UpdateAsync(NodeEntity node, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(node, cancellationToken);

    public Task<NodeEntity?> UpdateAsync(Guid adminId, NodeEntity node, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(adminId, node, cancellationToken);

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);

    public Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(adminId, id, cancellationToken);
}
