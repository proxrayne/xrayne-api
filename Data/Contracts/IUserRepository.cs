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
    /// Searches users.
    /// </summary>
    Task<OffsetPage<UserEntity>> SearchAsync(UserFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by identifier, or null when it does not exist.
    /// </summary>
    Task<UserEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    Task<UserEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a username exists.
    /// </summary>
    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user.
    /// </summary>
    Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user created by an administrator.
    /// </summary>
    Task<UserEntity> AddAsync(
        long adminId,
        UserEntity user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user while preserving its creator.
    /// </summary>
    Task<UserEntity?> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user and warehouse assignment while preserving its creator.
    /// </summary>
    Task<UserEntity> UpdateAsync(
        long id,
        UserEntity user,
        WarehouseEntity warehouse,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user by identifier.
    /// </summary>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

}
