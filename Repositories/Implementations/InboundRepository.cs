using Microsoft.EntityFrameworkCore;
using Contracts.Models;
using Contracts.Utilities;
using Repositories.Contracts;
using Repositories.Entities;

namespace Repositories.Implementations;

public sealed class InboundRepository(AppDbContext dbContext) : IInboundRepository
{
    private IQueryable<InboundEntity> _inboundsWithRelations => dbContext.Inbounds
        .Include(inbound => inbound.Users);

    public Task<List<InboundEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .OrderBy(inbound => inbound.DisplayName)
            .ToListAsync(ct);
    }

    public Task<List<InboundEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .Where(inbound => EF.Property<Guid>(inbound, "AdminId") == adminId)
            .OrderBy(inbound => inbound.DisplayName)
            .ToListAsync(ct);
    }

    public Task<CursorPage<InboundEntity>> SearchAsync(InboundFilter filter, CancellationToken ct = default)
    {
        return SearchCoreAsync(_inboundsWithRelations, filter, ct);
    }

    public Task<CursorPage<InboundEntity>> SearchAsync(Guid adminId, InboundFilter filter, CancellationToken ct = default)
    {
        var query = _inboundsWithRelations
            .Where(inbound => EF.Property<Guid>(inbound, "AdminId") == adminId);

        return SearchCoreAsync(query, filter, ct);
    }

    public Task<InboundEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .SingleOrDefaultAsync(inbound => inbound.Id == id, ct);
    }

    public Task<InboundEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .SingleOrDefaultAsync(
                inbound => inbound.Id == id && EF.Property<Guid>(inbound, "AdminId") == adminId,
                ct);
    }

    public async Task<InboundEntity> AddAsync(InboundEntity inbound, CancellationToken ct = default)
    {
        await dbContext.Inbounds.AddAsync(inbound, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(inbound).ReloadAsync(ct);

        return inbound;
    }

    public async Task<InboundEntity?> UpdateAsync(InboundEntity inbound, CancellationToken ct = default)
    {
        var exists = await dbContext.Inbounds.AnyAsync(item => item.Id == inbound.Id, ct);
        if (!exists)
        {
            return null;
        }

        inbound.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Inbounds.Update(inbound);
        await dbContext.SaveChangesAsync(ct);

        return inbound;
    }

    public async Task<InboundEntity?> UpdateAsync(Guid adminId, InboundEntity inbound, CancellationToken ct = default)
    {
        var exists = await dbContext.Inbounds.AnyAsync(
            item => item.Id == inbound.Id && EF.Property<Guid>(item, "AdminId") == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        inbound.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Inbounds.Update(inbound);
        await dbContext.SaveChangesAsync(ct);

        return inbound;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var inbound = await GetByIdAsync(id, ct);
        if (inbound is null)
        {
            return false;
        }

        dbContext.Inbounds.Remove(inbound);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid adminId, int id, CancellationToken ct = default)
    {
        var inbound = await GetByIdAsync(adminId, id, ct);
        if (inbound is null)
        {
            return false;
        }

        dbContext.Inbounds.Remove(inbound);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }



    private static async Task<CursorPage<InboundEntity>> SearchCoreAsync(IQueryable<InboundEntity> query, InboundFilter filter, CancellationToken ct)
    {
        query = ApplyFilter(query, filter);
        var totalCount = await query.CountAsync(ct);
        query = ApplyCursor(query, filter);
        query = ApplyOrder(query, filter.Order);

        var limit = CursorPagination.NormalizeLimit(filter.Limit);
        var items = await query.Take(limit + 1).ToListAsync(ct);
        var hasNextPage = items.Count > limit;
        if (hasNextPage)
        {
            items.RemoveAt(items.Count - 1);
        }

        var nextCursor = hasNextPage && items.Count > 0
            ? CursorPagination.CreateCursor(items[^1].CreatedAt, items[^1].Id)
            : null;

        return new CursorPage<InboundEntity>(items, nextCursor, hasNextPage, totalCount);
    }

    private static IQueryable<InboundEntity> ApplyFilter(IQueryable<InboundEntity> query, InboundFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(inbound =>
                EF.Functions.ILike(inbound.DisplayName, $"%{search}%")
                || EF.Functions.ILike(inbound.Config.Tag, $"%{search}%"));
        }

        if (filter.Enabled.HasValue)
        {
            query = query.Where(inbound => inbound.Enabled == filter.Enabled.Value);
        }

        if (filter.Protocol is { Count: > 0 })
        {
            query = query.Where(inbound => filter.Protocol.Contains(inbound.Config.Protocol));
        }

        if (filter.Network is { Count: > 0 })
        {
            query = query.Where(inbound => filter.Network.Contains(inbound.Config.StreamSettings.Network));
        }

        if (filter.Security is { Count: > 0 })
        {
            query = query.Where(inbound => filter.Security.Contains(inbound.Config.StreamSettings.Security));
        }

        return query;
    }

    private static IQueryable<InboundEntity> ApplyCursor(IQueryable<InboundEntity> query, InboundFilter filter)
    {
        var cursor = CursorPagination.TryReadCursor(filter.Cursor);
        if (cursor is null || !int.TryParse(cursor.Id, out var id))
        {
            return query;
        }

        return filter.Order is SortOrder.Desc
            ? query.Where(inbound => inbound.CreatedAt < cursor.CreatedAt
                || (inbound.CreatedAt == cursor.CreatedAt && inbound.Id < id))
            : query.Where(inbound => inbound.CreatedAt > cursor.CreatedAt
                || (inbound.CreatedAt == cursor.CreatedAt && inbound.Id > id));
    }

    private static IQueryable<InboundEntity> ApplyOrder(IQueryable<InboundEntity> query, SortOrder order)
    {
        return order is SortOrder.Desc
            ? query.OrderByDescending(inbound => inbound.CreatedAt).ThenByDescending(inbound => inbound.Id)
            : query.OrderBy(inbound => inbound.CreatedAt).ThenBy(inbound => inbound.Id);
    }
}
