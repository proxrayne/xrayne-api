using Microsoft.EntityFrameworkCore;
using XRayne.Repositories.Contracts;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories.Implementations;

public sealed class GeoResourceRepository(AppDbContext dbContext) : IGeoResourceRepository
{
    private IQueryable<GeoResourceEntity> GeoResourcesWithRelations => dbContext.GeoResources
        .Include(geoResource => geoResource.Node);

    public Task<List<GeoResourceEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .OrderBy(geoResource => geoResource.Filename)
            .ToListAsync(ct);
    }

    public Task<List<GeoResourceEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .Where(geoResource => EF.Property<Guid>(geoResource, "AdminId") == adminId)
            .OrderBy(geoResource => geoResource.Filename)
            .ToListAsync(ct);
    }

    public Task<GeoResourceEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .SingleOrDefaultAsync(geoResource => geoResource.Id == id, ct);
    }

    public Task<GeoResourceEntity?> GetByIdAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .SingleOrDefaultAsync(
                geoResource => geoResource.Id == id && EF.Property<Guid>(geoResource, "AdminId") == adminId,
                ct);
    }

    public async Task<GeoResourceEntity> AddAsync(GeoResourceEntity geoResource, CancellationToken ct = default)
    {
        await dbContext.GeoResources.AddAsync(geoResource, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(geoResource).ReloadAsync(ct);

        return geoResource;
    }

    public async Task<GeoResourceEntity?> UpdateAsync(GeoResourceEntity geoResource, CancellationToken ct = default)
    {
        var exists = await dbContext.GeoResources.AnyAsync(item => item.Id == geoResource.Id, ct);
        if (!exists)
        {
            return null;
        }

        geoResource.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.GeoResources.Update(geoResource);
        await dbContext.SaveChangesAsync(ct);

        return geoResource;
    }

    public async Task<GeoResourceEntity?> UpdateAsync(Guid adminId, GeoResourceEntity geoResource, CancellationToken ct = default)
    {
        var exists = await dbContext.GeoResources.AnyAsync(
            item => item.Id == geoResource.Id && EF.Property<Guid>(item, "AdminId") == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        geoResource.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.GeoResources.Update(geoResource);
        await dbContext.SaveChangesAsync(ct);

        return geoResource;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var geoResource = await GetByIdAsync(id, ct);
        if (geoResource is null)
        {
            return false;
        }

        dbContext.GeoResources.Remove(geoResource);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid adminId, long id, CancellationToken ct = default)
    {
        var geoResource = await GetByIdAsync(adminId, id, ct);
        if (geoResource is null)
        {
            return false;
        }

        dbContext.GeoResources.Remove(geoResource);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
