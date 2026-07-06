using Microsoft.EntityFrameworkCore;
using Contracts.Enums;
using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;

namespace Data.Implementations;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private IQueryable<User> _usersWithRelations => dbContext.Users
        .Include(user => user.Inbounds);

    public Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        return _usersWithRelations
            .OrderBy(user => user.Username)
            .ToListAsync(ct);
    }

    public Task<List<User>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return _usersWithRelations
            .Where(user => EF.Property<Guid>(user, "AdminId") == adminId)
            .OrderBy(user => user.Username)
            .ToListAsync(ct);
    }

    public Task<CursorPage<User>> SearchAsync(UserFilter filter, CancellationToken ct = default)
    {
        return SearchCoreAsync(_usersWithRelations, filter, ct);
    }

    public Task<CursorPage<User>> SearchAsync(Guid adminId, UserFilter filter, CancellationToken ct = default)
    {
        var query = _usersWithRelations
            .Where(user => EF.Property<Guid>(user, "AdminId") == adminId);

        return SearchCoreAsync(query, filter, ct);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _usersWithRelations
            .SingleOrDefaultAsync(user => user.Id == id, ct);
    }

    public Task<User?> GetByIdAsync(Guid adminId, Guid id, CancellationToken ct = default)
    {
        return _usersWithRelations
            .SingleOrDefaultAsync(
                user => user.Id == id && EF.Property<Guid>(user, "AdminId") == adminId,
                ct);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return _usersWithRelations
            .SingleOrDefaultAsync(user => user.Username == username, ct);
    }

    public Task<User?> GetByUsernameAsync(Guid adminId, string username, CancellationToken ct = default)
    {
        return _usersWithRelations
            .SingleOrDefaultAsync(
                user => user.Username == username && EF.Property<Guid>(user, "AdminId") == adminId,
                ct);
    }

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return dbContext.Users.AnyAsync(user => user.Username == username, ct);
    }

    public Task<bool> ExistsAsync(Guid adminId, string username, CancellationToken ct = default)
    {
        return dbContext.Users
            .AnyAsync(
                user => user.Username == username && EF.Property<Guid>(user, "AdminId") == adminId,
                ct);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        await dbContext.Users.AddAsync(user, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(user).ReloadAsync(ct);

        return user;
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken ct = default)
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

    public async Task<User?> UpdateAsync(Guid adminId, User user, CancellationToken ct = default)
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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
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

    public async Task<bool> DeleteAsync(Guid adminId, Guid id, CancellationToken ct = default)
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

    private static async Task<CursorPage<User>> SearchCoreAsync(IQueryable<User> query, UserFilter filter, CancellationToken ct)
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

        return new CursorPage<User>(items, nextCursor, hasNextPage, totalCount);
    }

    private static IQueryable<User> ApplyFilter(IQueryable<User> query, UserFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(user =>
                EF.Functions.ILike(user.Username, $"%{search}%")
                || EF.Functions.ILike(user.Note, $"%{search}%"));
        }

        if (filter.Status is { Count: > 0 })
        {
            query = query.Where(user => filter.Status.Contains(user.Status));
        }

        if (filter.LimitResetStrategy is { Count: > 0 })
        {
            query = query.Where(user => user.LimitResetStrategy.HasValue
                && filter.LimitResetStrategy.Contains(user.LimitResetStrategy.Value));
        }

        if (filter.Protocol is { Count: > 0 })
        {
            query = query.Where(user => filter.Protocol.Any(protocol => user.Options.ContainsKey(protocol)));
        }

        return query;
    }

    private static IQueryable<User> ApplyCursor(IQueryable<User> query, UserFilter filter)
    {
        var cursor = CursorPagination.TryReadCursor(filter.Cursor);
        if (cursor is null || !Guid.TryParse(cursor.Id, out var id))
        {
            return query;
        }

        return filter.Order is SortOrder.Desc
            ? query.Where(user => user.CreatedAt < cursor.CreatedAt
                || (user.CreatedAt == cursor.CreatedAt && user.Id.CompareTo(id) < 0))
            : query.Where(user => user.CreatedAt > cursor.CreatedAt
                || (user.CreatedAt == cursor.CreatedAt && user.Id.CompareTo(id) > 0));
    }

    private static IQueryable<User> ApplyOrder(IQueryable<User> query, SortOrder order)
    {
        return order is SortOrder.Desc
            ? query.OrderByDescending(user => user.CreatedAt).ThenByDescending(user => user.Id)
            : query.OrderBy(user => user.CreatedAt).ThenBy(user => user.Id);
    }
}
