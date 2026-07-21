using Data.Entities;

namespace Data.Contracts;

public interface IOutboundRepository
{
    Task<List<OutboundEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<OutboundEntity>> GetAllAsync(long adminId, CancellationToken cancellationToken = default);

    Task<List<OutboundEntity>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> GetByIdAsync(long adminId, long id, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> GetByNodeAndIdAsync(long nodeId, long id, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> GetByNodeAndTagAsync(long nodeId, string tag, CancellationToken cancellationToken = default);

    Task<OutboundEntity> AddAsync(OutboundEntity outbound, CancellationToken cancellationToken = default);

    Task<OutboundEntity> AddAsync(long adminId, long nodeId, OutboundEntity outbound, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> UpdateAsync(OutboundEntity outbound, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> UpdateAsync(long adminId, OutboundEntity outbound, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long adminId, long id, CancellationToken cancellationToken = default);
}
