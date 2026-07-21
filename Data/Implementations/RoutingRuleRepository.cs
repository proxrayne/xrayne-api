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

    public Task<List<RoutingRuleEntity>> GetAllAsync(long adminId, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .Where(routingRule => routingRule.AdminId == adminId)
            .OrderBy(routingRule => routingRule.Position)
            .ThenBy(routingRule => routingRule.Id)
            .ToListAsync(ct);
    }

    public Task<List<RoutingRuleEntity>> GetByNodeIdAsync(long nodeId, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .Where(routingRule => routingRule.NodeId == nodeId)
            .OrderBy(routingRule => routingRule.Position)
            .ThenBy(routingRule => routingRule.Id)
            .ToListAsync(ct);
    }

    public Task<RoutingRuleEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .SingleOrDefaultAsync(routingRule => routingRule.Id == id, ct);
    }

    public Task<RoutingRuleEntity?> GetByIdAsync(long adminId, long id, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .SingleOrDefaultAsync(
                routingRule => routingRule.Id == id && routingRule.AdminId == adminId,
                ct);
    }

    public Task<RoutingRuleEntity?> GetByNodeAndIdAsync(long nodeId, long id, CancellationToken ct = default)
    {
        return RoutingRulesWithRelations
            .SingleOrDefaultAsync(
                routingRule => routingRule.Id == id && routingRule.NodeId == nodeId,
                ct);
    }

    public async Task<RoutingRuleEntity?> GetByNodeAndRuleTagAsync(
        long nodeId,
        string ruleTag,
        CancellationToken ct = default)
    {
        var items = await RoutingRulesWithRelations
            .Where(routingRule => routingRule.NodeId == nodeId)
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
        long adminId,
        long nodeId,
        RoutingRuleEntity routingRule,
        CancellationToken ct = default)
    {
        await dbContext.RoutingRules.AddAsync(routingRule, ct);
        routingRule.AdminId = adminId;
        routingRule.NodeId = nodeId;
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

    public async Task<RoutingRuleEntity?> UpdateAsync(long adminId, RoutingRuleEntity routingRule, CancellationToken ct = default)
    {
        var exists = await dbContext.RoutingRules.AnyAsync(
            item => item.Id == routingRule.Id && item.AdminId == adminId,
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

    public async Task<List<RoutingRuleEntity>> SaveChangesAsync(
        long adminId,
        long nodeId,
        IReadOnlyCollection<RoutingRuleEntity> rulesToCreate,
        IReadOnlyCollection<RoutingRuleEntity> rulesToUpdate,
        IReadOnlyCollection<RoutingRuleEntity> rulesToDelete,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var rule in rulesToUpdate)
        {
            rule.UpdatedAt = now;
        }

        dbContext.RoutingRules.RemoveRange(rulesToDelete);
        dbContext.RoutingRules.UpdateRange(rulesToUpdate);
        await dbContext.RoutingRules.AddRangeAsync(rulesToCreate, ct);

        foreach (var rule in rulesToCreate)
        {
            rule.AdminId = adminId;
            rule.NodeId = nodeId;
        }

        await dbContext.SaveChangesAsync(ct);

        return await GetByNodeIdAsync(nodeId, ct);
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

    public async Task<bool> DeleteAsync(long adminId, long id, CancellationToken ct = default)
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
