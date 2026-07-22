using Contracts.Enums;
using Contracts.Models;
using Data.Entities;
using Data.Models;
using Xray.Config.Enums;

namespace Infrastructure.Services;

/// <summary>
/// Provides application operations for user connections.
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Searches connections assigned to a user.
    /// </summary>
    Task<OffsetPage<ConnectionEntity>> GetByUserIdAsync(
        long userId,
        ConnectionFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a connection by identifier.
    /// </summary>
    Task<ConnectionEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a user connection.
    /// </summary>
    Task<ConnectionEntity> CreateAsync(
        long userId,
        string name,
        XtlsFlow flow,
        EncryptionMethod method,
        DeviceVerificationMethod deviceVerificationMethod,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user connection.
    /// </summary>
    Task<ConnectionEntity> UpdateAsync(
        long id,
        string name,
        XtlsFlow flow,
        EncryptionMethod method,
        DeviceVerificationMethod deviceVerificationMethod,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Partially updates a user connection.
    /// </summary>
    Task<ConnectionEntity> PatchAsync(
        long id,
        ConnectionPatch patch,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a user connection.
    /// </summary>
    Task<ConnectionEntity> RevokeByIdAsync(long id, CancellationToken cancellationToken = default);
}
