using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for subscription users.
/// </summary>
public sealed class UserRepository(AppDbContext context) : IUserRepository
{
    private IQueryable<UserEntity> UsersWithRelations => context.Users
        .Include(user => user.Warehouse)
        .Include(user => user.Connections);

    /// <inheritdoc />
    public Task<List<UserEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return UsersWithRelations
            .OrderBy(user => user.Username)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public Task<OffsetPage<UserEntity>> SearchAsync(UserFilter filter, CancellationToken ct = default)
    {
        return SearchCoreAsync(UsersWithRelations, filter, ct);
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByIdOrDefaultAsync(long id, CancellationToken ct = default)
    {
        return UsersWithRelations
            .SingleOrDefaultAsync(user => user.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<UserEntity> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var user = await GetByIdOrDefaultAsync(id, ct);

        return user ?? throw new NotFoundException($"User '{id}' was not found.");
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return UsersWithRelations
            .SingleOrDefaultAsync(user => user.Username == username, ct);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return context.Users.AnyAsync(user => user.Username == username, ct);
    }

    /// <inheritdoc />
    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
        await context.Entry(user).ReloadAsync(ct);

        return user;
    }

    /// <inheritdoc />
    public async Task<UserEntity> AddAsync(
        long adminId,
        UserEntity user,
        CancellationToken cancellationToken = default)
    {
        user.WarehouseId = user.Warehouse.Id;
        user.AdminId = adminId;

        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserEntity?> UpdateAsync(UserEntity user, CancellationToken ct = default)
    {
        var existing = await GetByIdOrDefaultAsync(user.Id, ct);
        if (existing is null)
        {
            return null;
        }

        existing.Username = user.Username;
        existing.Note = user.Note;
        existing.DataLimit = user.DataLimit;
        existing.ConnectionLimit = user.ConnectionLimit;
        existing.Status = user.Status;
        existing.LimitResetStrategy = user.LimitResetStrategy;
        existing.LastTrafficReset = user.LastTrafficReset;
        existing.ExpireAt = user.ExpireAt;
        existing.OnHoldExpire = user.OnHoldExpire;
        existing.WarehouseId = user.WarehouseId;
        if (user.Warehouse is not null)
        {
            existing.Warehouse = user.Warehouse;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(ct);

        return existing;
    }

    /// <inheritdoc />
    public async Task<UserEntity> UpdateAsync(
        long id,
        UserEntity user,
        WarehouseEntity warehouse,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);

        existing.Note = user.Note;
        existing.DataLimit = user.DataLimit;
        existing.ConnectionLimit = user.ConnectionLimit;
        existing.Status = user.Status;
        existing.LimitResetStrategy = user.LimitResetStrategy;
        existing.ExpireAt = user.ExpireAt;
        existing.OnHoldExpire = user.OnHoldExpire;
        existing.Warehouse = warehouse;
        existing.WarehouseId = warehouse.Id;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var user = await GetByIdOrDefaultAsync(id, ct);
        if (user is null)
        {
            return false;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(ct);

        return true;
    }

    private static async Task<OffsetPage<UserEntity>> SearchCoreAsync(
        IQueryable<UserEntity> query,
        UserFilter filter,
        CancellationToken ct)
    {
        query = ApplyFilter(query, filter);
        var totalItems = await query.CountAsync(ct);
        var limit = OffsetPagination.NormalizeLimit(filter.Limit);
        var page = OffsetPagination.NormalizePage(filter.Page);
        var totalPages = OffsetPagination.CalculateTotalPages(totalItems, limit);
        var skip = (page - 1) * limit;

        var items = await ApplyOrder(query, filter.SortBy, filter.SortOrder)
            .Skip(skip)
            .Take(limit)
            .ToListAsync(ct);

        return new OffsetPage<UserEntity>(items, totalItems, page, totalPages);
    }

    private static IQueryable<UserEntity> ApplyFilter(IQueryable<UserEntity> query, UserFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(user => EF.Functions.ILike(user.Username, $"%{search}%"));
        }

        if (filter.Status is { Count: > 0 })
        {
            query = query.Where(user => filter.Status.Contains(user.Status));
        }

        return query;
    }

    private static IQueryable<UserEntity> ApplyOrder(
        IQueryable<UserEntity> query,
        UserSortBy sortBy,
        SortOrder sortOrder)
    {
        return (sortBy, sortOrder) switch
        {
            (UserSortBy.Status, SortOrder.Desc) => query
                .OrderByDescending(user => user.Status)
                .ThenBy(user => user.Id),
            (UserSortBy.Status, _) => query
                .OrderBy(user => user.Status)
                .ThenBy(user => user.Id),
            (UserSortBy.Traffic, SortOrder.Desc) => query
                .OrderByDescending(user => user.DataLimit)
                .ThenBy(user => user.Id),
            (UserSortBy.Traffic, _) => query
                .OrderBy(user => user.DataLimit)
                .ThenBy(user => user.Id),
            (UserSortBy.Connections, SortOrder.Desc) => query
                .OrderByDescending(user => user.Connections.Count(connection => !connection.Revoked))
                .ThenBy(user => user.Id),
            (UserSortBy.Connections, _) => query
                .OrderBy(user => user.Connections.Count(connection => !connection.Revoked))
                .ThenBy(user => user.Id),
            (UserSortBy.CreatedAt, SortOrder.Asc) => query
                .OrderBy(user => user.CreatedAt)
                .ThenBy(user => user.Id),
            (UserSortBy.CreatedAt, _) => query
                .OrderByDescending(user => user.CreatedAt)
                .ThenBy(user => user.Id),
            (UserSortBy.Username, SortOrder.Desc) => query
                .OrderByDescending(user => user.Username)
                .ThenBy(user => user.Id),
            _ => query
                .OrderBy(user => user.Username)
                .ThenBy(user => user.Id)
        };
    }
}
