using Contracts.Models;
using Data.Entities;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for user connections.
/// </summary>
public interface IConnectionRepository
{
    /// <summary>
    /// Gets a connection by identifier, or null when it does not exist.
    /// </summary>
    Task<ConnectionEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a connection by identifier.
    /// </summary>
    Task<ConnectionEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches connections assigned to a user.
    /// </summary>
    Task<OffsetPage<ConnectionEntity>> SearchByUserIdAsync(
        long userId,
        ConnectionFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a connection.
    /// </summary>
    Task<ConnectionEntity> AddAsync(ConnectionEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a connection.
    /// </summary>
    Task<ConnectionEntity> UpdateAsync(ConnectionEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a connection.
    /// </summary>
    Task<ConnectionEntity> RevokeByIdAsync(long id, CancellationToken cancellationToken = default);
}
