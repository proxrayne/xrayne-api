using Microsoft.EntityFrameworkCore;
using Data.Contracts;
using Data.Entities;

namespace Data.Implementations;

public sealed class OutboundRepository(AppDbContext dbContext) : IOutboundRepository
{
    private IQueryable<OutboundEntity> _outboundsWithRelations => dbContext.Outbounds
        .Include(outbound => outbound.Admin)
        .Include(outbound => outbound.Node);

    public Task<List<OutboundEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return _outboundsWithRelations
            .OrderBy(outbound => outbound.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<List<OutboundEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return _outboundsWithRelations
            .Where(outbound => EF.Property<Guid>(outbound, "AdminId") == adminId)
            .OrderBy(outbound => outbound.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<OutboundEntity>> GetByNodeIdAsync(long nodeId, CancellationToken ct = default)
    {
        var items = await _outboundsWithRelations
            .Where(outbound => EF.Property<long>(outbound, "NodeId") == nodeId)
            .ToListAsync(ct);

        return items
            .OrderBy(outbound => outbound.Tag ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(outbound => outbound.Id)
            .ToList();
    }

    public Task<OutboundEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _outboundsWithRelations
            .SingleOrDefaultAsync(outbound => outbound.Id == id, ct);
    }

    public Task<OutboundEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken ct = default)
    {
        return _outboundsWithRelations
            .SingleOrDefaultAsync(
                outbound => outbound.Id == id && EF.Property<Guid>(outbound, "AdminId") == adminId,
                ct);
    }

    public Task<OutboundEntity?> GetByNodeAndIdAsync(long nodeId, int id, CancellationToken ct = default)
    {
        return _outboundsWithRelations
            .SingleOrDefaultAsync(
                outbound => outbound.Id == id && EF.Property<long>(outbound, "NodeId") == nodeId,
                ct);
    }

    public async Task<OutboundEntity> AddAsync(OutboundEntity outbound, CancellationToken ct = default)
    {
        await dbContext.Outbounds.AddAsync(outbound, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(outbound).ReloadAsync(ct);

        return outbound;
    }

    public async Task<OutboundEntity> AddAsync(
        Guid adminId,
        long nodeId,
        OutboundEntity outbound,
        CancellationToken ct = default)
    {
        await dbContext.Outbounds.AddAsync(outbound, ct);
        dbContext.Entry(outbound).Property("AdminId").CurrentValue = adminId;
        dbContext.Entry(outbound).Property("NodeId").CurrentValue = nodeId;
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(outbound).ReloadAsync(ct);

        return outbound;
    }

    public async Task<OutboundEntity?> UpdateAsync(OutboundEntity outbound, CancellationToken ct = default)
    {
        var exists = await dbContext.Outbounds.AnyAsync(item => item.Id == outbound.Id, ct);
        if (!exists)
        {
            return null;
        }

        outbound.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Outbounds.Update(outbound);
        await dbContext.SaveChangesAsync(ct);

        return outbound;
    }

    public async Task<OutboundEntity?> UpdateAsync(Guid adminId, OutboundEntity outbound, CancellationToken ct = default)
    {
        var exists = await dbContext.Outbounds.AnyAsync(
            item => item.Id == outbound.Id && EF.Property<Guid>(item, "AdminId") == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        outbound.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Outbounds.Update(outbound);
        await dbContext.SaveChangesAsync(ct);

        return outbound;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var outbound = await GetByIdAsync(id, ct);
        if (outbound is null)
        {
            return false;
        }

        dbContext.Outbounds.Remove(outbound);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid adminId, int id, CancellationToken ct = default)
    {
        var outbound = await GetByIdAsync(adminId, id, ct);
        if (outbound is null)
        {
            return false;
        }

        dbContext.Outbounds.Remove(outbound);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
