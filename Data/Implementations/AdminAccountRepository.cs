using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for administrator accounts.
/// </summary>
public sealed class AdminAccountRepository(AppDbContext context) : IAdminAccountRepository
{
    /// <inheritdoc />
    public async Task<OffsetPage<AdminAccountEntity>> SearchAsync(
        AdminFilter filter,
        CancellationToken ct = default)
    {
        var query = ApplyFilter(
            context.AdminAccounts.Where(account => !account.IsDeleted),
            filter);

        var totalItems = await query.CountAsync(ct);
        var limit = OffsetPagination.NormalizeLimit(filter.Limit);
        var page = OffsetPagination.NormalizePage(filter.Page);
        var totalPages = OffsetPagination.CalculateTotalPages(totalItems, limit);
        var skip = (page - 1) * limit;

        var items = await query
            .OrderBy(account => account.Username)
            .ThenBy(account => account.Id)
            .Skip(skip)
            .Take(limit)
            .ToListAsync(ct);

        return new OffsetPage<AdminAccountEntity>(items, totalItems, page, totalPages);
    }

    /// <inheritdoc />
    public Task<AdminAccountEntity?> GetByIdOrDefaultAsync(long id, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Id == id, ct);
    }

    /// <inheritdoc />
    public Task<AdminAccountEntity?> GetActiveByIdOrDefaultAsync(long id, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Id == id && !account.IsDeleted, ct);
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var admin = await GetByIdOrDefaultAsync(id, ct);

        return RequiredEntity(admin, id.ToString());
    }

    /// <inheritdoc />
    public Task<AdminAccountEntity?> GetByUsernameOrDefaultAsync(string username, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Username == username, ct);
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity> GetActiveByIdAsync(long id, CancellationToken ct = default)
    {
        var admin = await GetActiveByIdOrDefaultAsync(id, ct);

        return RequiredEntity(admin, id.ToString());
    }


    /// <inheritdoc />
    public Task<AdminAccountEntity?> GetActiveByUsernameAsync(string username, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Username == username && !account.IsDeleted, ct);
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var admin = await GetByUsernameOrDefaultAsync(username, ct);

        return RequiredEntity(admin, username);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .AnyAsync(account => account.Username == username, ct);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string username, long exceptId, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .AnyAsync(account => account.Id != exceptId && account.Username == username, ct);
    }

    /// <inheritdoc />
    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .AnyAsync(account => account.Email == email, ct);
    }

    /// <inheritdoc />
    public Task<bool> EmailExistsAsync(string email, long exceptId, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .AnyAsync(account => account.Id != exceptId && account.Email == email, ct);
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity> AddAsync(AdminAccountEntity account, CancellationToken ct = default)
    {
        await context.AdminAccounts.AddAsync(account, ct);
        await context.SaveChangesAsync(ct);
        await context.Entry(account).ReloadAsync(ct);

        return account;
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity?> SetLastLoginAsync(
        long id,
        DateTimeOffset lastLoginAt,
        CancellationToken ct = default)
    {
        var account = await GetActiveByIdOrDefaultAsync(id, ct);
        if (account is null)
        {
            return null;
        }

        account.LastLoginAt = lastLoginAt;

        await context.SaveChangesAsync(ct);

        return account;
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity> ChangePasswordAsync(
        long id,
        string passwordHash,
        CancellationToken ct = default)
    {
        var account = await GetActiveByIdAsync(id, ct);

        account.PasswordHash = passwordHash;
        await context.SaveChangesAsync(ct);

        return account;
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity> UpdateAsync(
        long id,
        AdminAccountPatch account,
        CancellationToken ct = default)
    {
        var existing = await GetActiveByIdAsync(id, ct);
        var hasPatch = false;

        if (account.Username.IsSpecified)
        {
            existing.Username = account.Username.SpecifiedValue!.Trim();
            hasPatch = true;
        }

        if (account.Email.IsSpecified)
        {
            existing.Email = string.IsNullOrWhiteSpace(account.Email.SpecifiedValue)
                ? null
                : account.Email.SpecifiedValue.Trim();
            hasPatch = true;
        }

        if (account.Permissions.IsSpecified)
        {
            existing.Permissions = account.Permissions.SpecifiedValue;
            hasPatch = true;
        }

        if (!hasPatch)
        {
            return existing;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(ct);

        return existing;
    }

    /// <inheritdoc />
    public async Task<AdminAccountEntity> ChangePermissionsAsync(
        long id,
        AdminPermission permissions,
        CancellationToken ct = default)
    {
        var account = await GetActiveByIdAsync(id, ct);

        account.Permissions = permissions;

        await context.SaveChangesAsync(ct);

        return account;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var account = await GetActiveByIdAsync(id, ct);

        account.IsDeleted = true;
        account.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    private static IQueryable<AdminAccountEntity> ApplyFilter(
        IQueryable<AdminAccountEntity> query,
        AdminFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(account =>
                EF.Functions.ILike(account.Username, $"%{search}%")
                || (account.Email != null && EF.Functions.ILike(account.Email, $"%{search}%")));
        }

        return query;
    }

    private static AdminAccountEntity RequiredEntity(AdminAccountEntity? entity, string id)
    {
        return entity ?? throw new NotFoundException($"Admin '{id}' not found.");
    }
}
