using Contracts.Models;
using Data.Entities;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for subscription users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all users.
    /// </summary>
    Task<List<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users owned by an administrator.
    /// </summary>
    Task<List<UserEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users.
    /// </summary>
    Task<CursorPage<UserEntity>> SearchAsync(UserFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users owned by an administrator.
    /// </summary>
    Task<CursorPage<UserEntity>> SearchAsync(Guid adminId, UserFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    Task<UserEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by owner and identifier.
    /// </summary>
    Task<UserEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by owner and username.
    /// </summary>
    Task<UserEntity?> GetByUsernameAsync(Guid adminId, string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a username exists.
    /// </summary>
    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a username exists for an administrator.
    /// </summary>
    Task<bool> ExistsAsync(Guid adminId, string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user.
    /// </summary>
    Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user.
    /// </summary>
    Task<UserEntity?> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user owned by an administrator.
    /// </summary>
    Task<UserEntity?> UpdateAsync(Guid adminId, UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by identifier.
    /// </summary>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by owner and identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default);
}
