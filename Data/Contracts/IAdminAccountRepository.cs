using Contracts.Enums;
using Contracts.Models;
using Data.Models;
using Data.Entities;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for administrator accounts.
/// </summary>
public interface IAdminAccountRepository
{
    /// <summary>
    /// Searches active administrator accounts with offset pagination.
    /// </summary>
    Task<OffsetPage<AdminAccountEntity>> SearchAsync(
        AdminFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an administrator account by identifier, including soft-deleted accounts or null.
    /// </summary>
    Task<AdminAccountEntity?> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an administrator account by identifier, including soft-deleted accounts.
    /// </summary>
    Task<AdminAccountEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an active administrator account by identifier or null.
    /// </summary>
    Task<AdminAccountEntity?> GetActiveByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an active administrator account by identifier .
    /// </summary>
    Task<AdminAccountEntity> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an administrator account by username, including soft-deleted accounts.
    /// </summary>
    Task<AdminAccountEntity?> GetByUsernameOrDefaultAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an administrator account by username, including soft-deleted accounts.
    /// </summary>
    Task<AdminAccountEntity> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an active administrator account by username.
    /// </summary>
    Task<AdminAccountEntity?> GetActiveByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the username is reserved by any administrator account.
    /// </summary>
    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the username is reserved by another administrator account.
    /// </summary>
    Task<bool> ExistsAsync(
        string username,
        Guid exceptId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the email is reserved by any administrator account.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the email is reserved by another administrator account.
    /// </summary>
    Task<bool> EmailExistsAsync(
        string email,
        Guid exceptId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new administrator account.
    /// </summary>
    Task<AdminAccountEntity> AddAsync(AdminAccountEntity account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last successful login timestamp for an active administrator account.
    /// </summary>
    Task<AdminAccountEntity?> SetLastLoginAsync(Guid id, DateTimeOffset lastLoginAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password hash for an active administrator account.
    /// </summary>
    Task<AdminAccountEntity> ChangePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch updates an active administrator account using only specified fields.
    /// </summary>
    Task<AdminAccountEntity> UpdateAsync(
        Guid id,
        AdminAccountPatch account,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the permissions for an active administrator account.
    /// </summary>
    Task<AdminAccountEntity> ChangePermissionsAsync(Guid id, AdminPermission permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an active administrator account.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
