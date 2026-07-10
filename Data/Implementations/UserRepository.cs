using Microsoft.EntityFrameworkCore;
using Contracts.Enums;
using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for subscription users.
/// </summary>
public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private IQueryable<UserEntity> UsersWithRelations => dbContext.Users
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
    public Task<List<UserEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return UsersWithRelations
            .Where(user => EF.Property<Guid>(user, "AdminId") == adminId)
            .OrderBy(user => user.Username)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public Task<OffsetPage<UserEntity>> SearchAsync(UserFilter filter, CancellationToken ct = default)
    {
        return SearchCoreAsync(UsersWithRelations, filter, ct);
    }

    /// <inheritdoc />
    public Task<OffsetPage<UserEntity>> SearchAsync(Guid adminId, UserFilter filter, CancellationToken ct = default)
    {
        var query = UsersWithRelations
            .Where(user => EF.Property<Guid>(user, "AdminId") == adminId);

        return SearchCoreAsync(query, filter, ct);
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return UsersWithRelations
            .SingleOrDefaultAsync(user => user.Id == id, ct);
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        return UsersWithRelations
            .SingleOrDefaultAsync(
                user => user.Id == id && EF.Property<Guid>(user, "AdminId") == adminId,
                ct);
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return UsersWithRelations
            .SingleOrDefaultAsync(user => user.Username == username, ct);
    }

    /// <inheritdoc />
    public Task<UserEntity?> GetByUsernameAsync(Guid adminId, string username, CancellationToken ct = default)
    {
        return UsersWithRelations
            .SingleOrDefaultAsync(
                user => user.Username == username && EF.Property<Guid>(user, "AdminId") == adminId,
                ct);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return dbContext.Users.AnyAsync(user => user.Username == username, ct);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(Guid adminId, string username, CancellationToken ct = default)
    {
        return dbContext.Users
            .AnyAsync(
                user => user.Username == username && EF.Property<Guid>(user, "AdminId") == adminId,
                ct);
    }

    /// <inheritdoc />
    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct = default)
    {
        await dbContext.Users.AddAsync(user, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(user).ReloadAsync(ct);

        return user;
    }

    /// <inheritdoc />
    public async Task<UserEntity> AddAsync(
        Guid adminId,
        UserEntity user,
        WarehouseEntity warehouse,
        CancellationToken cancellationToken = default)
    {
        user.Warehouse = warehouse;
        await dbContext.Users.AddAsync(user, cancellationToken);
        dbContext.Entry(user).Property("AdminId").CurrentValue = adminId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(adminId, user.Id, cancellationToken) ?? user;
    }

    /// <inheritdoc />
    public async Task<UserEntity?> UpdateAsync(UserEntity user, CancellationToken ct = default)
    {
        var exists = await dbContext.Users.AnyAsync(item => item.Id == user.Id, ct);
        if (!exists)
        {
            return null;
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);

        return user;
    }

    /// <inheritdoc />
    public async Task<UserEntity?> UpdateAsync(Guid adminId, UserEntity user, CancellationToken ct = default)
    {
        var exists = await dbContext.Users.AnyAsync(
            item => item.Id == user.Id && EF.Property<Guid>(item, "AdminId") == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(ct);

        return user;
    }

    /// <inheritdoc />
    public async Task<UserEntity?> UpdateAsync(
        Guid adminId,
        long id,
        UserEntity user,
        WarehouseEntity warehouse,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(adminId, id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Note = user.Note;
        existing.DataLimit = user.DataLimit;
        existing.ConnectionLimit = user.ConnectionLimit;
        existing.Status = user.Status;
        existing.LimitResetStrategy = user.LimitResetStrategy;
        existing.ExpireAt = user.ExpireAt;
        existing.OnHoldExpire = user.OnHoldExpire;
        existing.Warehouse = warehouse;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(adminId, id, ct);
        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(ct);

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
                .OrderByDescending(user => user.Connections.Count)
                .ThenBy(user => user.Id),
            (UserSortBy.Connections, _) => query
                .OrderBy(user => user.Connections.Count)
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
