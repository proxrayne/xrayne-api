using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for user connections.
/// </summary>
public sealed class ConnectionRepository(AppDbContext context) : IConnectionRepository
{
    private IQueryable<ConnectionEntity> ConnectionsWithRelations => context.Connections
        .Include(x => x.OperatingSystem)
        .Include(x => x.Application);

    public Task<ConnectionEntity?> GetByIdOrDefaultAsync(long id, CancellationToken ct = default)
    {
        return ConnectionsWithRelations.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<ConnectionEntity> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await GetByIdOrDefaultAsync(id, ct);

        return entity ?? throw new NotFoundException($"Connection with Id = '{id}' not found.");
    }

    public async Task<OffsetPage<ConnectionEntity>> SearchByUserIdAsync(
        long userId,
        ConnectionFilter filter,
        CancellationToken ct = default)
    {
        var query = ApplyFilter(ConnectionsWithRelations.Where(x => x.UserId == userId), filter);
        var totalItems = await query.CountAsync(ct);
        var limit = OffsetPagination.NormalizeLimit(filter.Limit);
        var page = OffsetPagination.NormalizePage(filter.Page);
        var totalPages = OffsetPagination.CalculateTotalPages(totalItems, limit);
        var skip = (page - 1) * limit;

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(limit)
            .ToListAsync(ct);

        return new OffsetPage<ConnectionEntity>(items, totalItems, page, totalPages);
    }

    public async Task<ConnectionEntity> RevokeByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);

        if (!entity.Revoked)
        {
            entity.Revoked = true;
            entity.RevokedAt = DateTimeOffset.UtcNow;

            await context.SaveChangesAsync(ct);
        }

        return entity;
    }

    public async Task<ConnectionEntity> UpdateAsync(ConnectionEntity entity, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(entity.Id, ct);

        existing.Name = entity.Name;
        existing.UserAgent = entity.UserAgent;
        existing.Password = entity.Password;
        existing.Uuid = entity.Uuid;
        existing.Flow = entity.Flow;
        existing.Method = entity.Method;
        existing.DeviceInfo = entity.DeviceInfo;
        existing.DeviceVerificationMethod = entity.DeviceVerificationMethod;
        existing.OnlineAt = entity.OnlineAt;
        existing.ConnectedAt = entity.ConnectedAt;
        existing.SubscriptionUpdatedAt = entity.SubscriptionUpdatedAt;
        existing.RevokedAt = entity.RevokedAt;
        existing.Revoked = entity.Revoked;
        existing.UserId = entity.UserId;
        existing.OperationSystemId = entity.OperationSystemId;
        existing.ApplicationId = entity.ApplicationId;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return existing;
    }

    public async Task<ConnectionEntity> AddAsync(ConnectionEntity entity, CancellationToken ct = default)
    {
        await context.AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
        await context.Entry(entity).ReloadAsync(ct);

        return entity;
    }

    private static IQueryable<ConnectionEntity> ApplyFilter(
        IQueryable<ConnectionEntity> query,
        ConnectionFilter filter)
    {
        if (!filter.IncludeRevoked)
        {
            query = query.Where(x => !x.Revoked);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(x =>
                (x.Name != null && EF.Functions.ILike(x.Name, $"%{search}%"))
                || (x.UserAgent != null && EF.Functions.ILike(x.UserAgent, $"%{search}%")));
        }

        return query;
    }
}
