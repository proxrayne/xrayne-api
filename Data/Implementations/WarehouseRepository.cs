using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for connection warehouses.
/// </summary>
public sealed class WarehouseRepository(AppDbContext dbContext) : IWarehouseRepository
{
    private IQueryable<WarehouseEntity> WarehousesWithRelations => dbContext.Warehouses
        .Include(warehouse => warehouse.Inbounds)
        .ThenInclude(inbound => inbound.Node)
        .Include(warehouse => warehouse.Users);

    /// <inheritdoc />
    public async Task<OffsetPage<WarehouseEntity>> SearchAsync(
        Guid adminId,
        WarehouseFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = WarehousesWithRelations
            .Where(warehouse => EF.Property<Guid>(warehouse, "AdminId") == adminId);

        query = ApplyFilter(query, filter);

        var totalItems = await query.CountAsync(cancellationToken);
        var limit = OffsetPagination.NormalizeLimit(filter.Limit);
        var page = OffsetPagination.NormalizePage(filter.Page);
        var totalPages = OffsetPagination.CalculateTotalPages(totalItems, limit);
        var skip = (page - 1) * limit;

        var items = await query
            .OrderBy(warehouse => warehouse.Name)
            .ThenBy(warehouse => warehouse.Id)
            .Skip(skip)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new OffsetPage<WarehouseEntity>(items, totalItems, page, totalPages);
    }

    /// <inheritdoc />
    public Task<WarehouseEntity?> GetByIdAsync(
        Guid adminId,
        long id,
        CancellationToken cancellationToken = default)
    {
        return WarehousesWithRelations
            .SingleOrDefaultAsync(
                warehouse => warehouse.Id == id && EF.Property<Guid>(warehouse, "AdminId") == adminId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<InboundEntity>> GetInboundOptionsAsync(
        Guid adminId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var inbounds = await dbContext.Inbounds
            .Include(inbound => inbound.Node)
            .Where(inbound => EF.Property<Guid>(inbound, "AdminId") == adminId)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            inbounds = inbounds
                .Where(inbound =>
                    inbound.Tag.Contains(value, StringComparison.OrdinalIgnoreCase)
                    || inbound.Node.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return inbounds
            .OrderBy(inbound => inbound.Node.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(inbound => inbound.Tag, StringComparer.OrdinalIgnoreCase)
            .ThenBy(inbound => inbound.Id)
            .ToList();
    }

    /// <inheritdoc />
    public Task<List<InboundEntity>> GetInboundsByIdsAsync(
        Guid adminId,
        IReadOnlyCollection<int> inboundIds,
        CancellationToken cancellationToken = default)
    {
        if (inboundIds.Count == 0)
        {
            return Task.FromResult(new List<InboundEntity>());
        }

        return dbContext.Inbounds
            .Include(inbound => inbound.Node)
            .Where(inbound =>
                inboundIds.Contains(inbound.Id)
                && EF.Property<Guid>(inbound, "AdminId") == adminId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WarehouseEntity> AddAsync(
        Guid adminId,
        WarehouseEntity warehouse,
        IReadOnlyCollection<InboundEntity> inbounds,
        CancellationToken cancellationToken = default)
    {
        warehouse.Inbounds = inbounds.ToList();
        await dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
        dbContext.Entry(warehouse).Property("AdminId").CurrentValue = adminId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(adminId, warehouse.Id, cancellationToken) ?? warehouse;
    }

    /// <inheritdoc />
    public async Task<WarehouseEntity?> UpdateAsync(
        Guid adminId,
        long id,
        WarehouseEntity warehouse,
        IReadOnlyCollection<InboundEntity> inbounds,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(adminId, id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Name = warehouse.Name;
        existing.Note = warehouse.Note;
        existing.Enabled = warehouse.Enabled;
        existing.Inbounds.Clear();
        existing.Inbounds.AddRange(inbounds);
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
    {
        var warehouse = await GetByIdAsync(adminId, id, cancellationToken);
        if (warehouse is null)
        {
            return false;
        }

        dbContext.Warehouses.Remove(warehouse);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public Task<bool> HasUsersAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
    {
        return dbContext.Warehouses.AnyAsync(
            warehouse =>
                warehouse.Id == id
                && EF.Property<Guid>(warehouse, "AdminId") == adminId
                && warehouse.Users.Any(),
            cancellationToken);
    }

    private static IQueryable<WarehouseEntity> ApplyFilter(
        IQueryable<WarehouseEntity> query,
        WarehouseFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(warehouse => EF.Functions.ILike(warehouse.Name, $"%{search}%"));
        }

        if (filter.Enabled.HasValue)
        {
            query = query.Where(warehouse => warehouse.Enabled == filter.Enabled.Value);
        }

        if (filter.InboundIds is { Count: > 0 })
        {
            var inboundIds = filter.InboundIds.Distinct().ToArray();
            query = query.Where(warehouse => warehouse.Inbounds.Any(inbound => inboundIds.Contains(inbound.Id)));
        }

        return query;
    }
}
