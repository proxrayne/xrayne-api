using Contracts.Models;
using Repositories.Entities;

namespace Repositories.Contracts;

public interface IInboundRepository
{
    Task<List<InboundEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<InboundEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<CursorPage<InboundEntity>> SearchAsync(InboundFilter filter, CancellationToken cancellationToken = default);

    Task<CursorPage<InboundEntity>> SearchAsync(Guid adminId, InboundFilter filter, CancellationToken cancellationToken = default);

    Task<InboundEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<InboundEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken cancellationToken = default);

    Task<InboundEntity> AddAsync(InboundEntity inbound, CancellationToken cancellationToken = default);

    Task<InboundEntity?> UpdateAsync(InboundEntity inbound, CancellationToken cancellationToken = default);

    Task<InboundEntity?> UpdateAsync(Guid adminId, InboundEntity inbound, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, int id, CancellationToken cancellationToken = default);
}
