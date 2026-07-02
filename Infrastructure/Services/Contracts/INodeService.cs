using XRayne.Repositories.Entities;

namespace XRayne.Infrastructure.Services;

public interface INodeService
{
    Task<List<NodeEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<NodeEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<NodeEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<NodeEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default);

    Task<NodeEntity> AddAsync(NodeEntity node, CancellationToken cancellationToken = default);

    Task<NodeEntity> AddAsync(Guid adminId, NodeEntity node, CancellationToken cancellationToken = default);

    Task<NodeEntity?> UpdateAsync(NodeEntity node, CancellationToken cancellationToken = default);

    Task<NodeEntity?> UpdateAsync(Guid adminId, NodeEntity node, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default);
}
