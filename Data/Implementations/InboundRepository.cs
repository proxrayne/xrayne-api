using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

public sealed class InboundRepository(AppDbContext context) : IInboundRepository
{
    private IQueryable<InboundEntity> _inboundsWithRelations => context.Inbounds
        .Include(inbound => inbound.Admin)
        .Include(inbound => inbound.Node);

    public Task<List<InboundEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .OrderBy(inbound => inbound.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<List<InboundEntity>> GetAllAsync(long adminId, CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .Where(inbound => inbound.AdminId == adminId)
            .OrderBy(inbound => inbound.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<InboundEntity>> GetByNodeIdAsync(long nodeId, CancellationToken ct = default)
    {
        var items = await _inboundsWithRelations
            .Where(inbound => inbound.NodeId == nodeId)
            .ToListAsync(ct);

        return items
            .OrderBy(inbound => inbound.Tag, StringComparer.Ordinal)
            .ThenBy(inbound => inbound.Id)
            .ToList();
    }

    public Task<CursorPage<InboundEntity>> SearchAsync(InboundFilter filter, CancellationToken ct = default)
    {
        return SearchCoreAsync(_inboundsWithRelations, filter, ct);
    }

    public Task<CursorPage<InboundEntity>> SearchAsync(long adminId, InboundFilter filter, CancellationToken ct = default)
    {
        var query = _inboundsWithRelations
            .Where(inbound => inbound.AdminId == adminId);

        return SearchCoreAsync(query, filter, ct);
    }

    public Task<InboundEntity?> GetByIdOrDefaultAsync(long id, CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .SingleOrDefaultAsync(inbound => inbound.Id == id, ct);
    }

    public async Task<InboundEntity> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var inbound = await GetByIdOrDefaultAsync(id, ct);

        return RequireEntity(inbound, id);
    }

    public Task<InboundEntity?> GetByIdAsync(long adminId, long id, CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .SingleOrDefaultAsync(
                inbound => inbound.Id == id && inbound.AdminId == adminId,
                ct);
    }

    public Task<InboundEntity?> GetByNodeAndIdAsync(long nodeId, long id, CancellationToken ct = default)
    {
        return _inboundsWithRelations
            .SingleOrDefaultAsync(
                inbound => inbound.Id == id && inbound.NodeId == nodeId,
                ct);
    }

    public async Task<InboundEntity?> GetByNodeAndTagAsync(long nodeId, string tag, CancellationToken ct = default)
    {
        var items = await _inboundsWithRelations
            .Where(inbound => inbound.NodeId == nodeId)
            .ToListAsync(ct);

        return items.SingleOrDefault(inbound => string.Equals(inbound.Tag, tag, StringComparison.Ordinal));
    }

    public async Task<InboundEntity> AddAsync(InboundEntity inbound, CancellationToken ct = default)
    {
        await context.Inbounds.AddAsync(inbound, ct);
        await context.SaveChangesAsync(ct);
        await context.Entry(inbound).ReloadAsync(ct);

        return inbound;
    }

    public async Task<InboundEntity> AddAsync(
        long adminId,
        long nodeId,
        InboundEntity inbound,
        CancellationToken ct = default)
    {
        await context.Inbounds.AddAsync(inbound, ct);
        inbound.AdminId = adminId;
        inbound.NodeId = nodeId;
        await context.SaveChangesAsync(ct);
        await context.Entry(inbound).ReloadAsync(ct);

        return inbound;
    }

    public async Task<InboundEntity?> UpdateAsync(InboundEntity inbound, CancellationToken ct = default)
    {
        var exists = await context.Inbounds.AnyAsync(item => item.Id == inbound.Id, ct);
        if (!exists)
        {
            return null;
        }

        inbound.UpdatedAt = DateTimeOffset.UtcNow;
        context.Inbounds.Update(inbound);
        await context.SaveChangesAsync(ct);

        return inbound;
    }

    public async Task<InboundEntity?> UpdateAsync(long adminId, InboundEntity inbound, CancellationToken ct = default)
    {
        var exists = await context.Inbounds.AnyAsync(
            item => item.Id == inbound.Id && item.AdminId == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        inbound.UpdatedAt = DateTimeOffset.UtcNow;
        context.Inbounds.Update(inbound);
        await context.SaveChangesAsync(ct);

        return inbound;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var inbound = await GetByIdOrDefaultAsync(id, ct);
        if (inbound is null)
        {
            return false;
        }

        context.Inbounds.Remove(inbound);
        await context.SaveChangesAsync(ct);

        return true;
    }

    private static async Task<CursorPage<InboundEntity>> SearchCoreAsync(IQueryable<InboundEntity> query, InboundFilter filter, CancellationToken ct)
    {
        query = ApplyDatabaseFilter(query, filter);
        var filteredItems = ApplyConfigFilter(await query.ToListAsync(ct), filter).ToList();
        var totalCount = filteredItems.Count;
        var orderedItems = ApplyOrder(ApplyCursor(filteredItems, filter), filter.Order).ToList();

        var limit = CursorPagination.NormalizeLimit(filter.Limit);
        var items = orderedItems.Take(limit + 1).ToList();
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

    private static IQueryable<InboundEntity> ApplyDatabaseFilter(IQueryable<InboundEntity> query, InboundFilter filter)
    {
        if (filter.Enabled.HasValue)
        {
            query = query.Where(inbound => inbound.Enabled == filter.Enabled.Value);
        }

        return query;
    }

    private static IEnumerable<InboundEntity> ApplyConfigFilter(IEnumerable<InboundEntity> query, InboundFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(inbound => inbound.Tag.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.Protocol is { Count: > 0 })
        {
            query = query.Where(inbound => filter.Protocol.Contains(inbound.Protocol));
        }

        if (filter.Network is { Count: > 0 })
        {
            query = query.Where(inbound => inbound.Network.HasValue && filter.Network.Contains(inbound.Network.Value));
        }

        if (filter.Security is { Count: > 0 })
        {
            query = query.Where(inbound => inbound.Security.HasValue && filter.Security.Contains(inbound.Security.Value));
        }

        return query;
    }

    private static IEnumerable<InboundEntity> ApplyCursor(IEnumerable<InboundEntity> query, InboundFilter filter)
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

    private static IOrderedEnumerable<InboundEntity> ApplyOrder(IEnumerable<InboundEntity> query, SortOrder order)
    {
        return order is SortOrder.Desc
            ? query.OrderByDescending(inbound => inbound.CreatedAt).ThenByDescending(inbound => inbound.Id)
            : query.OrderBy(inbound => inbound.CreatedAt).ThenBy(inbound => inbound.Id);
    }

    //
    private InboundEntity RequireEntity(InboundEntity? entity, long id)
    {
        return entity ?? throw new NotFoundException($"Inbound '{id}' not found.");
    }

}
