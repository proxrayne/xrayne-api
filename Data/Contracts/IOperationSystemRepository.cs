using Data.Entities;
using Data.Models;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for operating systems.
/// </summary>
public interface IOperationSystemRepository
{
    /// <summary>
    /// Gets all operating systems with images.
    /// </summary>
    Task<List<OperationSystemEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an operating system by identifier or returns null.
    /// </summary>
    Task<OperationSystemEntity?> GetByIdOrDefaultAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an operating system by identifier.
    /// </summary>
    Task<OperationSystemEntity> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets operating systems by identifiers.
    /// </summary>
    Task<List<OperationSystemEntity>> GetByIdsAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an operating system.
    /// </summary>
    Task<OperationSystemEntity> AddAsync(
        OperationSystemEntity operationSystem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an operating system.
    /// </summary>
    Task<OperationSystemEntity> UpdateAsync(
        string id,
        OperationSystemEntity operationSystem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch updates an operating system using only specified fields.
    /// </summary>
    Task<OperationSystemEntity> UpdateAsync(
        string id,
        OperationSystemPatch operationSystem,
        CancellationToken cancellationToken = default);
}
