using XRayne.Repositories.Contracts;
using XRayne.Repositories.Entities;

namespace XRayne.Infrastructure.Services;

public sealed class RoutingRuleService(IRoutingRuleRepository repository) : IRoutingRuleService
{
    public Task<List<RoutingRuleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<List<RoutingRuleEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default)
        => repository.GetAllAsync(adminId, cancellationToken);

    public Task<RoutingRuleEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, cancellationToken);

    public Task<RoutingRuleEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(adminId, id, cancellationToken);

    public Task<RoutingRuleEntity> AddAsync(RoutingRuleEntity routingRule, CancellationToken cancellationToken = default)
        => repository.AddAsync(routingRule, cancellationToken);

    public Task<RoutingRuleEntity?> UpdateAsync(RoutingRuleEntity routingRule, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(routingRule, cancellationToken);

    public Task<RoutingRuleEntity?> UpdateAsync(Guid adminId, RoutingRuleEntity routingRule, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(adminId, routingRule, cancellationToken);

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);

    public Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(adminId, id, cancellationToken);
}
