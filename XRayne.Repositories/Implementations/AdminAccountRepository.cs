using Microsoft.EntityFrameworkCore;
using XRayne.Contracts.Enums;
using XRayne.Repositories.Contracts;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories.Implementations;

public sealed class AdminAccountRepository(AppDbContext dbContext) : IAdminAccountRepository
{
    public Task<AdminAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return dbContext.AdminAccounts
            .SingleOrDefaultAsync(account => account.Id == id, ct);
    }

    public Task<AdminAccount?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return dbContext.AdminAccounts
            .SingleOrDefaultAsync(account => account.Username == username, ct);
    }

    public Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return dbContext.AdminAccounts
            .AnyAsync(account => account.Username == username, ct);
    }

    public async Task<AdminAccount> AddAsync(AdminAccount account, CancellationToken ct = default)
    {
        await dbContext.AdminAccounts.AddAsync(account, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(account).ReloadAsync(ct);

        return account;
    }

    public async Task<AdminAccount?> SetLastLoginAsync(
        Guid id,
        DateTimeOffset lastLoginAt,
        CancellationToken ct = default)
    {
        var account = await GetByIdAsync(id, ct);
        if (account is null)
        {
            return null;
        }

        account.LastLoginAt = lastLoginAt;
        
        await dbContext.SaveChangesAsync(ct);

        return account;
    }

    public async Task<AdminAccount?> ChangePasswordAsync(
        Guid id,
        string passwordHash,
        CancellationToken ct = default)
    {
        var account = await GetByIdAsync(id, ct);
        if (account is null)
        {
            return null;
        }

        account.PasswordHash = passwordHash;
        await dbContext.SaveChangesAsync(ct);

        return account;
    }

    public async Task<AdminAccount?> ChangePermissionsAsync(
        Guid id,
        AdminPermission permissions,
        CancellationToken ct = default)
    {
        var account = await GetByIdAsync(id, ct);
        if (account is null)
        {
            return null;
        }

        account.Permissions = permissions;
        await dbContext.SaveChangesAsync(ct);

        return account;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var account = await GetByIdAsync(id, ct);
        if (account is null)
        {
            return false;
        }

        dbContext.AdminAccounts.Remove(account);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
