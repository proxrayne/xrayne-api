using XRayne.Contracts.Enums;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories.Admins;

public interface IAdminAccountRepository
{
    Task<AdminAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AdminAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);

    Task AddAsync(AdminAccount account, CancellationToken cancellationToken = default);

    Task<AdminAccount?> SetLastLoginAsync(Guid id, DateTimeOffset lastLoginAt, CancellationToken cancellationToken = default);

    Task<AdminAccount?> ChangePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default);

    Task<AdminAccount?> ChangePermissionsAsync(Guid id, AdminPermission permissions, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
