using Contracts.Enums;
using Contracts.Exceptions;
using Data.Contracts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

public sealed class GeoResourceRepository(AppDbContext dbContext) : IGeoResourceRepository
{
    private IQueryable<GeoResourceEntity> GeoResourcesWithRelations => dbContext.GeoResources
        .Include(geoResource => geoResource.Node)
        .Include(geoResource => geoResource.Admin);

    public Task<List<GeoResourceEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .OrderBy(geoResource => geoResource.Filename)
            .ToListAsync(ct);
    }

    public Task<List<GeoResourceEntity>> GetAllAsync(long adminId, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .Where(geoResource => geoResource.AdminId == adminId)
            .OrderBy(geoResource => geoResource.Filename)
            .ToListAsync(ct);
    }

    public Task<List<GeoResourceEntity>> GetAllAsync(long adminId, long nodeId, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .Where(geoResource =>
                geoResource.AdminId == adminId &&
                geoResource.NodeId == nodeId)
            .OrderBy(geoResource => geoResource.Filename)
            .ToListAsync(ct);
    }

    public Task<List<GeoResourceEntity>> GetAllByNodeIdAsync(long nodeId, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .Where(geoResource => geoResource.NodeId == nodeId)
            .OrderBy(geoResource => geoResource.Filename)
            .ToListAsync(ct);
    }

    public Task<List<GeoResourceEntity>> GetDueAutoUpdateIdsAsync(DateTimeOffset now, CancellationToken ct = default)
    {
        return dbContext.GeoResources
            .Where(geoResource =>
                geoResource.Url != null &&
                geoResource.NextRunAt != null &&
                geoResource.NextRunAt <= now &&
                (geoResource.Status == GeoResourceStatus.Success ||
                    geoResource.Status == GeoResourceStatus.Error))
            .OrderBy(geoResource => geoResource.NextRunAt)
            .ToListAsync(ct);
    }

    public Task<GeoResourceEntity?> GetByIdOrDefaultAsync(long id, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .SingleOrDefaultAsync(geoResource => geoResource.Id == id, ct);
    }

    public async Task<GeoResourceEntity> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var resource = await GetByIdOrDefaultAsync(id, ct);

        return RequireEntity(resource, id);
    }


    public Task<GeoResourceEntity?> GetByIdOrDefaultAsync(long adminId, long id, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .SingleOrDefaultAsync(
                geoResource => geoResource.Id == id && geoResource.AdminId == adminId,
                ct);
    }

    public async Task<GeoResourceEntity> GetByIdAsync(long adminId, long id, CancellationToken ct = default)
    {
        var resource = await GetByIdOrDefaultAsync(adminId, id, ct);

        return RequireEntity(resource, id);
    }

    public Task<GeoResourceEntity?> GetByIdOrDefaultAsync(long adminId, long nodeId, long id, CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .SingleOrDefaultAsync(
                geoResource =>
                    geoResource.Id == id &&
                    geoResource.AdminId == adminId &&
                    geoResource.NodeId == nodeId,
                ct);
    }

    public async Task<GeoResourceEntity> GetByNodeIdAsync(long nodeId, long id, CancellationToken ct = default)
    {
        var entity = await GeoResourcesWithRelations
           .SingleOrDefaultAsync(x => x.Id == id && x.NodeId == nodeId, ct);

        return RequireEntity(entity, id);
    }

    public Task<GeoResourceEntity?> GetByFilenameAsync(
        long adminId,
        long nodeId,
        string fileName,
        CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .SingleOrDefaultAsync(
                geoResource =>
                    geoResource.AdminId == adminId &&
                    geoResource.NodeId == nodeId &&
                    geoResource.Filename.ToLower() == fileName.ToLower(),
                ct);
    }

    public Task<GeoResourceEntity?> GetByFilenameAsync(
        long nodeId,
        string fileName,
        CancellationToken ct = default)
    {
        return GeoResourcesWithRelations
            .SingleOrDefaultAsync(
                geoResource =>
                    geoResource.NodeId == nodeId &&
                    geoResource.Filename.ToLower() == fileName.ToLower(),
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

    public async Task<GeoResourceEntity?> UpdateAsync(long adminId, GeoResourceEntity geoResource, CancellationToken ct = default)
    {
        var exists = await dbContext.GeoResources.AnyAsync(
            item => item.Id == geoResource.Id && item.AdminId == adminId,
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
        var geoResource = await GetByIdOrDefaultAsync(id, ct);
        if (geoResource is null)
        {
            return false;
        }

        dbContext.GeoResources.Remove(geoResource);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(long adminId, long id, CancellationToken ct = default)
    {
        var geoResource = await GetByIdOrDefaultAsync(adminId, id, ct);
        if (geoResource is null)
        {
            return false;
        }

        dbContext.GeoResources.Remove(geoResource);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    // 

    private GeoResourceEntity RequireEntity(GeoResourceEntity? entity, long id)
    {
        return entity ?? throw new NotFoundException($"Geo resource with Id = {id} not found.");
    }
}
