using Microsoft.EntityFrameworkCore;
using XRayne.Repositories.Contracts;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories.Implementations;

public sealed class NodeRepository(AppDbContext dbContext) : INodeRepository
{
    private IQueryable<NodeEntity> NodesWithRelations => dbContext.Nodes
        .Include(node => node.Inbounds)
        .Include(node => node.Outbounds)
        .Include(node => node.RoutingRules);

    public Task<List<NodeEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return NodesWithRelations
            .OrderBy(node => node.Name)
            .ToListAsync(ct);
    }

    public Task<List<NodeEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return NodesWithRelations
            .Where(node => EF.Property<Guid>(node, "AdminId") == adminId)
            .OrderBy(node => node.Name)
            .ToListAsync(ct);
    }

    public Task<NodeEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return NodesWithRelations
            .SingleOrDefaultAsync(node => node.Id == id, ct);
    }

    public Task<NodeEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        return NodesWithRelations
            .SingleOrDefaultAsync(
                node => node.Id == id && EF.Property<Guid>(node, "AdminId") == adminId,
                ct);
    }

    public async Task<NodeEntity> AddAsync(NodeEntity node, CancellationToken ct = default)
    {
        await dbContext.Nodes.AddAsync(node, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(node).ReloadAsync(ct);

        return node;
    }

    public async Task<NodeEntity?> UpdateAsync(NodeEntity node, CancellationToken ct = default)
    {
        var exists = await dbContext.Nodes.AnyAsync(item => item.Id == node.Id, ct);
        if (!exists)
        {
            return null;
        }

        node.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Nodes.Update(node);
        await dbContext.SaveChangesAsync(ct);

        return node;
    }

    public async Task<NodeEntity?> UpdateAsync(Guid adminId, NodeEntity node, CancellationToken ct = default)
    {
        var exists = await dbContext.Nodes.AnyAsync(
            item => item.Id == node.Id && EF.Property<Guid>(item, "AdminId") == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        node.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Nodes.Update(node);
        await dbContext.SaveChangesAsync(ct);

        return node;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var node = await GetByIdAsync(id, ct);
        if (node is null)
        {
            return false;
        }

        dbContext.Nodes.Remove(node);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        var node = await GetByIdAsync(adminId, id, ct);
        if (node is null)
        {
            return false;
        }

        dbContext.Nodes.Remove(node);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
