using Microsoft.EntityFrameworkCore;
using Data.Contracts;
using Data.Entities;

namespace Data.Implementations;

public sealed class RoutingRuleRepository(AppDbContext dbContext) : IRoutingRuleRepository
{
    private IQueryable<RoutingRuleEntity> RoutingRulesWithRelations => dbContext.RoutingRules
        .Include(routingRule => routingRule.Node);

    public Task<List<RoutingRuleEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .OrderBy(routingRule => routingRule.Position)
            .ThenBy(routingRule => routingRule.Id)
            .ToListAsync(ct);
    }

    public Task<List<RoutingRuleEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .Where(routingRule => EF.Property<Guid>(routingRule, "AdminId") == adminId)
            .OrderBy(routingRule => routingRule.Position)
            .ThenBy(routingRule => routingRule.Id)
            .ToListAsync(ct);
    }

    public Task<List<RoutingRuleEntity>> GetByNodeIdAsync(long nodeId, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .Where(routingRule => EF.Property<long>(routingRule, "NodeId") == nodeId)
            .OrderBy(routingRule => routingRule.Position)
            .ThenBy(routingRule => routingRule.Id)
            .ToListAsync(ct);
    }

    public Task<RoutingRuleEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .SingleOrDefaultAsync(routingRule => routingRule.Id == id, ct);
    }

    public Task<RoutingRuleEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .SingleOrDefaultAsync(
                routingRule => routingRule.Id == id && EF.Property<Guid>(routingRule, "AdminId") == adminId,
                ct);
    }

    public Task<RoutingRuleEntity?> GetByNodeAndIdAsync(long nodeId, long id, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .SingleOrDefaultAsync(
                routingRule => routingRule.Id == id && EF.Property<long>(routingRule, "NodeId") == nodeId,
                ct);
    }

    public async Task<RoutingRuleEntity?> GetByNodeAndRuleTagAsync(
        long nodeId,
        string ruleTag,
        CancellationToken ct = default)
    {
        var items = await RoutingRulesWithRelations
            .Where(routingRule => EF.Property<long>(routingRule, "NodeId") == nodeId)
            .ToListAsync(ct);

        return items.SingleOrDefault(rule => string.Equals(rule.RuleTag, ruleTag, StringComparison.Ordinal));
    }

    public async Task<RoutingRuleEntity> AddAsync(RoutingRuleEntity routingRule, CancellationToken ct = default)
    {
        await dbContext.RoutingRules.AddAsync(routingRule, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(routingRule).ReloadAsync(ct);

        return routingRule;
    }

    public async Task<RoutingRuleEntity> AddAsync(
        Guid adminId,
        long nodeId,
        RoutingRuleEntity routingRule,
        CancellationToken ct = default)
    {
        await dbContext.RoutingRules.AddAsync(routingRule, ct);
        dbContext.Entry(routingRule).Property("AdminId").CurrentValue = adminId;
        dbContext.Entry(routingRule).Property("NodeId").CurrentValue = nodeId;
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(routingRule).ReloadAsync(ct);

        return routingRule;
    }

    public async Task<RoutingRuleEntity?> UpdateAsync(RoutingRuleEntity routingRule, CancellationToken ct = default)
    {
        var exists = await dbContext.RoutingRules.AnyAsync(item => item.Id == routingRule.Id, ct);
        if (!exists)
        {
            return null;
        }

        routingRule.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.RoutingRules.Update(routingRule);
        await dbContext.SaveChangesAsync(ct);

        return routingRule;
    }

    public async Task<RoutingRuleEntity?> UpdateAsync(Guid adminId, RoutingRuleEntity routingRule, CancellationToken ct = default)
    {
        var exists = await dbContext.RoutingRules.AnyAsync(
            item => item.Id == routingRule.Id && EF.Property<Guid>(item, "AdminId") == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        routingRule.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.RoutingRules.Update(routingRule);
        await dbContext.SaveChangesAsync(ct);

        return routingRule;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var routingRule = await GetByIdAsync(id, ct);
        if (routingRule is null)
        {
            return false;
        }

        dbContext.RoutingRules.Remove(routingRule);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        var routingRule = await GetByIdAsync(adminId, id, ct);
        if (routingRule is null)
        {
            return false;
        }

        dbContext.RoutingRules.Remove(routingRule);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
