using Data.Entities;

namespace Data.Contracts;

public interface IGeoResourceRepository
{
    Task<List<GeoResourceEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<GeoResourceEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<List<GeoResourceEntity>> GetAllAsync(Guid adminId, long nodeId, CancellationToken cancellationToken = default);

    Task<List<GeoResourceEntity>> GetDueAutoUpdateAsync(DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdAsync(Guid adminId, long nodeId, long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByFilenameAsync(Guid adminId, long nodeId, string fileName, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity> AddAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> UpdateAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> UpdateAsync(Guid adminId, GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default);
}
