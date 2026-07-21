using Data.Entities;
using Data.Models;

namespace Data.Contracts;

/// <summary>
/// Provides persistence operations for client applications.
/// </summary>
public interface IApplicationRepository
{
    /// <summary>
    /// Gets all applications with images and operating systems.
    /// </summary>
    Task<List<ApplicationEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an application by identifier or returns null.
    /// </summary>
    Task<ApplicationEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an application by identifier.
    /// </summary>
    Task<ApplicationEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an application with its linked operating systems.
    /// </summary>
    Task<ApplicationEntity> AddAsync(
        ApplicationEntity application,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch updates an application and replaces its linked operating systems.
    /// </summary>
    Task<ApplicationEntity> UpdateAsync(
        int id,
        ApplicationEntity application,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch updates an application using only specified fields.
    /// </summary>
    Task<ApplicationEntity> UpdateAsync(
        int id,
        ApplicationPatch application,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an application.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
