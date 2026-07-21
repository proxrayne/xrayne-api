using Data.Entities;

namespace Data.Contracts;

public interface INodeRepository
{
    Task<List<NodeEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<NodeEntity>> GetAllAsync(long adminId, CancellationToken cancellationToken = default);

    Task<NodeEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<NodeEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default);

    Task<NodeEntity> GetByIdAsync(long adminId, long id, CancellationToken cancellationToken = default);

    Task<bool> ExistByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<NodeEntity?> GetByIdOrDefaultAsync(long adminId, long id, CancellationToken cancellationToken = default);

    Task<NodeEntity> AddAsync(NodeEntity node, CancellationToken cancellationToken = default);

    Task<NodeEntity> AddAsync(long adminId, NodeEntity node, CancellationToken cancellationToken = default);

    Task<NodeEntity?> UpdateAsync(NodeEntity node, CancellationToken cancellationToken = default);

    Task<NodeEntity?> UpdateAsync(long adminId, NodeEntity node, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long adminId, long id, CancellationToken cancellationToken = default);
}
