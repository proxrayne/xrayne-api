using XRayne.Repositories.Entities;

namespace XRayne.Repositories.Contracts;

public interface IRoutingRuleRepository
{
    Task<List<RoutingRuleEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<RoutingRuleEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<RoutingRuleEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<RoutingRuleEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default);

    Task<RoutingRuleEntity> AddAsync(RoutingRuleEntity routingRule, CancellationToken cancellationToken = default);

    Task<RoutingRuleEntity?> UpdateAsync(RoutingRuleEntity routingRule, CancellationToken cancellationToken = default);

    Task<RoutingRuleEntity?> UpdateAsync(Guid adminId, RoutingRuleEntity routingRule, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default);
}
