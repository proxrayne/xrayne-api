using Contracts.Enums;
using Data.Entities;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for administrator accounts.
/// </summary>
public interface IAdminAccountRepository
{
    /// <summary>
    /// Gets an administrator account by identifier, including soft-deleted accounts.
    /// </summary>
    Task<AdminAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an active administrator account by identifier.
    /// </summary>
    Task<AdminAccount?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an administrator account by username, including soft-deleted accounts.
    /// </summary>
    Task<AdminAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an active administrator account by username.
    /// </summary>
    Task<AdminAccount?> GetActiveByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the username is reserved by any administrator account.
    /// </summary>
    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new administrator account.
    /// </summary>
    Task<AdminAccount> AddAsync(AdminAccount account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last successful login timestamp for an active administrator account.
    /// </summary>
    Task<AdminAccount?> SetLastLoginAsync(Guid id, DateTimeOffset lastLoginAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password hash for an active administrator account.
    /// </summary>
    Task<AdminAccount?> ChangePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the permissions for an active administrator account.
    /// </summary>
    Task<AdminAccount?> ChangePermissionsAsync(Guid id, AdminPermission permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an active administrator account.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
