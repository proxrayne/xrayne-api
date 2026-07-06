using Data.Entities;

namespace Data.Contracts;

public interface IOutboundRepository
{
    Task<List<OutboundEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<OutboundEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken cancellationToken = default);

    Task<OutboundEntity> AddAsync(OutboundEntity outbound, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> UpdateAsync(OutboundEntity outbound, CancellationToken cancellationToken = default);

    Task<OutboundEntity?> UpdateAsync(Guid adminId, OutboundEntity outbound, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, int id, CancellationToken cancellationToken = default);
}
