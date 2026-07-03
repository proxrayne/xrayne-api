using Repositories.Contracts;
using Repositories.Entities;

namespace Infrastructure.Services;

public sealed class GeoResourceService(IGeoResourceRepository repository) : IGeoResourceService
{
    public Task<List<GeoResourceEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<List<GeoResourceEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default)
        => repository.GetAllAsync(adminId, cancellationToken);

    public Task<GeoResourceEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, cancellationToken);

    public Task<GeoResourceEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(adminId, id, cancellationToken);

    public Task<GeoResourceEntity> AddAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default)
        => repository.AddAsync(geoResource, cancellationToken);

    public Task<GeoResourceEntity?> UpdateAsync(GeoResourceEntity geoResource, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(geoResource, cancellationToken);

    public Task<GeoResourceEntity?> UpdateAsync(Guid adminId, GeoResourceEntity geoResource, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(adminId, geoResource, cancellationToken);

    public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);

    public Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(adminId, id, cancellationToken);
}
