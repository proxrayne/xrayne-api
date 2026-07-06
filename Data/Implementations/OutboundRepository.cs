using Microsoft.EntityFrameworkCore;
using Data.Contracts;
using Data.Entities;

namespace Data.Implementations;

public sealed class OutboundRepository(AppDbContext dbContext) : IOutboundRepository
{
    public Task<List<OutboundEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return dbContext.Outbounds
            .OrderBy(outbound => outbound.Position)
            .ToListAsync(ct);
    }

    public Task<List<OutboundEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return dbContext.Outbounds
            .Where(outbound => EF.Property<Guid>(outbound, "AdminId") == adminId)
            .OrderBy(outbound => outbound.Position)
            .ToListAsync(ct);
    }

    public Task<OutboundEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return dbContext.Outbounds
            .SingleOrDefaultAsync(outbound => outbound.Id == id, ct);
    }

    public Task<OutboundEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken ct = default)
    {
        return dbContext.Outbounds
            .SingleOrDefaultAsync(
                outbound => outbound.Id == id && EF.Property<Guid>(outbound, "AdminId") == adminId,
                ct);
    }

    public async Task<OutboundEntity> AddAsync(OutboundEntity outbound, CancellationToken ct = default)
    {
        await dbContext.Outbounds.AddAsync(outbound, ct);
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
