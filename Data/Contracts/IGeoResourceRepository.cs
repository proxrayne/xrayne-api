using Data.Entities;

namespace Data.Contracts;

public interface IGeoResourceRepository
{
    Task<List<GeoResourceEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<GeoResourceEntity>> GetAllAsync(long adminId, CancellationToken cancellationToken = default);

    Task<List<GeoResourceEntity>> GetAllAsync(long adminId, long nodeId, CancellationToken cancellationToken = default);
    Task<List<GeoResourceEntity>> GetAllByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);

    Task<List<GeoResourceEntity>> GetDueAutoUpdateIdsAsync(DateTimeOffset now, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default);
    Task<GeoResourceEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdOrDefaultAsync(long adminId, long id, CancellationToken cancellationToken = default);
    Task<GeoResourceEntity> GetByIdAsync(long adminId, long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdOrDefaultAsync(long adminId, long nodeId, long id, CancellationToken cancellationToken = default);
    Task<GeoResourceEntity> GetByNodeIdAsync(long nodeId, long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByFilenameAsync(long adminId, long nodeId, string fileName, CancellationToken cancellationToken = default);
    Task<GeoResourceEntity?> GetByFilenameAsync(long nodeId, string fileName, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity> AddAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> UpdateAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> UpdateAsync(long adminId, GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long adminId, long id, CancellationToken cancellationToken = default);
}
