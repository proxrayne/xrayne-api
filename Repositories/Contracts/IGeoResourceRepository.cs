using Repositories.Entities;

namespace Repositories.Contracts;

public interface IGeoResourceRepository
{
    Task<List<GeoResourceEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<GeoResourceEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity> AddAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> UpdateAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<GeoResourceEntity?> UpdateAsync(Guid adminId, GeoResourceEntity geoResource, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default);
}
