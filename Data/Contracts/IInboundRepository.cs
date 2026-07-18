using Contracts.Models;
using Data.Entities;

namespace Data.Contracts;

public interface IInboundRepository
{
    Task<List<InboundEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<InboundEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<List<InboundEntity>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);

    Task<CursorPage<InboundEntity>> SearchAsync(InboundFilter filter, CancellationToken cancellationToken = default);

    Task<CursorPage<InboundEntity>> SearchAsync(Guid adminId, InboundFilter filter, CancellationToken cancellationToken = default);

    Task<InboundEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default);
    Task<InboundEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<InboundEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default);

    Task<InboundEntity?> GetByNodeAndIdAsync(long nodeId, long id, CancellationToken cancellationToken = default);

    Task<InboundEntity?> GetByNodeAndTagAsync(long nodeId, string tag, CancellationToken cancellationToken = default);

    Task<InboundEntity> AddAsync(InboundEntity inbound, CancellationToken cancellationToken = default);

    Task<InboundEntity> AddAsync(Guid adminId, long nodeId, InboundEntity inbound, CancellationToken cancellationToken = default);

    Task<InboundEntity?> UpdateAsync(InboundEntity inbound, CancellationToken cancellationToken = default);

    Task<InboundEntity?> UpdateAsync(Guid adminId, InboundEntity inbound, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
