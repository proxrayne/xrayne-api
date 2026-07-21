using Contracts.Exceptions;
using Data.Contracts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

public sealed class NodeRepository(AppDbContext context) : INodeRepository
{
    private IQueryable<NodeEntity> NodesWithRelations => context.Nodes
        .Include(node => node.Admin)
        .Include(node => node.Inbounds)
        .Include(node => node.Outbounds)
        .Include(node => node.RoutingRules);

    public Task<List<NodeEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return NodesWithRelations
            .OrderBy(node => node.Name)
            .ToListAsync(ct);
    }

    public Task<List<NodeEntity>> GetAllAsync(long adminId, CancellationToken ct = default)
    {
        return NodesWithRelations
            .Where(node => node.AdminId == adminId)
            .OrderBy(node => node.Name)
            .ToListAsync(ct);
    }

    public async Task<NodeEntity> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var node = await GetByIdOrDefaultAsync(id, ct);

        return Required(node, id);
    }

    public Task<bool> ExistByIdAsync(long id, CancellationToken ct = default)
    {
        return context.Nodes.AnyAsync(x => x.Id == id, ct);
    }

    public Task<NodeEntity?> GetByIdOrDefaultAsync(long id, CancellationToken ct = default)
    {
        return NodesWithRelations
            .SingleOrDefaultAsync(node => node.Id == id, ct);
    }

    public async Task<NodeEntity> GetByIdAsync(long adminId, long id, CancellationToken ct = default)
    {
        var node = await GetByIdOrDefaultAsync(adminId, id, ct);

        return Required(node, id);
    }

    public Task<NodeEntity?> GetByIdOrDefaultAsync(long adminId, long id, CancellationToken ct = default)
    {
        return NodesWithRelations
           .SingleOrDefaultAsync(
               node => node.Id == id && node.AdminId == adminId,
               ct);
    }

    public async Task<NodeEntity> AddAsync(NodeEntity node, CancellationToken ct = default)
    {
        await context.Nodes.AddAsync(node, ct);
        await context.SaveChangesAsync(ct);
        await context.Entry(node).ReloadAsync(ct);

        return node;
    }

    public async Task<NodeEntity> AddAsync(long adminId, NodeEntity node, CancellationToken ct = default)
    {
        await context.Nodes.AddAsync(node, ct);
        node.AdminId = adminId;
        await context.SaveChangesAsync(ct);
        await context.Entry(node).ReloadAsync(ct);

        return node;
    }

    public async Task<NodeEntity?> UpdateAsync(NodeEntity node, CancellationToken ct = default)
    {
        var exists = await context.Nodes.AnyAsync(item => item.Id == node.Id, ct);
        if (!exists)
        {
            return null;
        }

        node.UpdatedAt = DateTimeOffset.UtcNow;
        context.Nodes.Update(node);
        await context.SaveChangesAsync(ct);

        return node;
    }

    public async Task<NodeEntity?> UpdateAsync(long adminId, NodeEntity node, CancellationToken ct = default)
    {
        var exists = await context.Nodes.AnyAsync(
            item => item.Id == node.Id && item.AdminId == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        node.UpdatedAt = DateTimeOffset.UtcNow;
        context.Nodes.Update(node);
        await context.SaveChangesAsync(ct);

        return node;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var node = await GetByIdAsync(id, ct);
        if (node is null)
        {
            return false;
        }

        context.Nodes.Remove(node);
        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(long adminId, long id, CancellationToken ct = default)
    {
        var node = await GetByIdAsync(adminId, id, ct);
        if (node is null)
        {
            return false;
        }

        context.Nodes.Remove(node);
        await context.SaveChangesAsync(ct);

        return true;
    }

    private NodeEntity Required(NodeEntity? node, long id)
    {
        return node ?? throw new NotFoundException($"Node '{id}' was not found.");
    }
}
