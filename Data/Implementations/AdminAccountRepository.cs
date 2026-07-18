using Contracts.Enums;
using Contracts.Exceptions;
using Data.Contracts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

public sealed class AdminAccountRepository(AppDbContext context) : IAdminAccountRepository
{
    public Task<AdminAccountEntity?> GetByIdOrDefaultAsync(Guid id, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Id == id, ct);
    }

    public Task<AdminAccountEntity?> GetActiveByIdOrDefaultAsync(Guid id, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Id == id && !account.IsDeleted, ct);
    }

    public async Task<AdminAccountEntity> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var admin = await GetByIdAsync(id, ct);

        return RequiredEntity(admin, id.ToString());
    }

    public Task<AdminAccountEntity?> GetByUsernameOrDefaultAsync(string username, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Username == username, ct);
    }

    public async Task<AdminAccountEntity> GetActiveByIdAsync(Guid id, CancellationToken ct = default)
    {
        var admin = await GetActiveByIdOrDefaultAsync(id, ct);

        return RequiredEntity(admin, id.ToString());
    }


    public Task<AdminAccountEntity?> GetActiveByUsernameAsync(string username, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .SingleOrDefaultAsync(account => account.Username == username && !account.IsDeleted, ct);
    }

    public async Task<AdminAccountEntity> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var admin = await GetByUsernameOrDefaultAsync(username, ct);

        return RequiredEntity(admin, username);
    }

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return context.AdminAccounts
            .AnyAsync(account => account.Username == username, ct);
    }

    public async Task<AdminAccountEntity> AddAsync(AdminAccountEntity account, CancellationToken ct = default)
    {
        await context.AdminAccounts.AddAsync(account, ct);
        await context.SaveChangesAsync(ct);
        await context.Entry(account).ReloadAsync(ct);

        return account;
    }

    public async Task<AdminAccountEntity?> SetLastLoginAsync(
        Guid id,
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

    public async Task<AdminAccountEntity> ChangePasswordAsync(
        Guid id,
        string passwordHash,
        CancellationToken ct = default)
    {
        var account = await GetActiveByIdAsync(id, ct);

        account.PasswordHash = passwordHash;
        await context.SaveChangesAsync(ct);

        return account;
    }

    public async Task<AdminAccountEntity> ChangePermissionsAsync(
        Guid id,
        AdminPermission permissions,
        CancellationToken ct = default)
    {
        var account = await GetActiveByIdAsync(id, ct);

        account.Permissions = permissions;

        await context.SaveChangesAsync(ct);

        return account;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await GetActiveByIdAsync(id, ct);

        account.IsDeleted = true;
        account.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    private AdminAccountEntity RequiredEntity(AdminAccountEntity? entity, string id)
    {
        return entity ?? throw new NotFoundException($"Admin '{id}' not found.");
    }
}
