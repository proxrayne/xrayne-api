using Data.Entities;
using Data.Models;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for subscription hosts.
/// </summary>
public interface IHostRepository
{
    /// <summary>
    /// Gets hosts owned by an administrator in display order.
    /// </summary>
    Task<List<HostEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one host by owner and identifier, or null when it does not exist.
    /// </summary>
    Task<HostEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one host by owner and identifier.
    /// </summary>
    Task<HostEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inbound options available for host assignment.
    /// </summary>
    Task<List<InboundEntity>> GetInboundOptionsAsync(
        long adminId,
        string? search,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an inbound belongs to the administrator.
    /// </summary>
    Task<bool> InboundExistsAsync(long inboundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a host for an administrator.
    /// </summary>
    Task<HostEntity> AddAsync(long adminId, HostEntity host, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a host owned by an administrator.
    /// </summary>
    Task<HostEntity?> UpdateAsync(
        long id,
        HostEntity host,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Partially updates a host owned by an administrator.
    /// </summary>
    Task<HostEntity?> PatchAsync(
        long id,
        HostPatch patch,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces host ordering for an administrator.
    /// </summary>
    Task<List<HostEntity>> UpdateOrderAsync(
        IReadOnlyList<long> hostIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a host owned by an administrator.
    /// </summary>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
