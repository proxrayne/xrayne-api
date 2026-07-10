using Contracts.Models;
using Data.Entities;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for connection warehouses.
/// </summary>
public interface IWarehouseRepository
{
    /// <summary>
    /// Searches warehouses owned by an administrator.
    /// </summary>
    Task<OffsetPage<WarehouseEntity>> SearchAsync(
        Guid adminId,
        WarehouseFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a warehouse by owner and identifier.
    /// </summary>
    Task<WarehouseEntity?> GetByIdAsync(
        Guid adminId,
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inbound options available to warehouses.
    /// </summary>
    Task<List<InboundEntity>> GetInboundOptionsAsync(
        Guid adminId,
        string? search,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inbounds by owner and identifiers.
    /// </summary>
    Task<List<InboundEntity>> GetInboundsByIdsAsync(
        Guid adminId,
        IReadOnlyCollection<long> inboundIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a warehouse for an administrator.
    /// </summary>
    Task<WarehouseEntity> AddAsync(
        Guid adminId,
        WarehouseEntity warehouse,
        IReadOnlyCollection<InboundEntity> inbounds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a warehouse owned by an administrator.
    /// </summary>
    Task<WarehouseEntity?> UpdateAsync(
        Guid adminId,
        long id,
        WarehouseEntity warehouse,
        IReadOnlyCollection<InboundEntity> inbounds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a warehouse owned by an administrator.
    /// </summary>
    Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a warehouse has assigned users.
    /// </summary>
    Task<bool> HasUsersAsync(Guid adminId, long id, CancellationToken cancellationToken = default);
}
