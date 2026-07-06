using Microsoft.EntityFrameworkCore;
using Data.Contracts;
using Data.Entities;

namespace Data.Implementations;

public sealed class CertificateRepository(AppDbContext dbContext) : ICertificateRepository
{
    private IQueryable<CertificateEntity> CertificatesWithRelations => dbContext.Certificates
        .Include(certificate => certificate.Node);

    public Task<List<CertificateEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return CertificatesWithRelations
            .OrderBy(certificate => certificate.Domain)
            .ToListAsync(ct);
    }

    public Task<List<CertificateEntity>> GetAllAsync(Guid adminId, CancellationToken ct = default)
    {
        return CertificatesWithRelations
            .Where(certificate => EF.Property<Guid>(certificate, "AdminId") == adminId)
            .OrderBy(certificate => certificate.Domain)
            .ToListAsync(ct);
    }

    public Task<List<CertificateEntity>> GetAllAsync(Guid adminId, long nodeId, CancellationToken ct = default)
    {
        return CertificatesWithRelations
            .Where(certificate =>
                EF.Property<Guid>(certificate, "AdminId") == adminId &&
                EF.Property<long>(certificate, "NodeId") == nodeId)
            .OrderBy(certificate => certificate.Domain)
            .ToListAsync(ct);
    }

    public Task<CertificateEntity?> GetByDomainAsync(
        Guid adminId,
        long nodeId,
        string domain,
        CancellationToken ct = default)
    {
        return CertificatesWithRelations
            .SingleOrDefaultAsync(
                certificate =>
                    EF.Property<Guid>(certificate, "AdminId") == adminId &&
                    EF.Property<long>(certificate, "NodeId") == nodeId &&
                    certificate.Domain.ToLower() == domain,
                ct);
    }

    public Task<CertificateEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return CertificatesWithRelations
            .SingleOrDefaultAsync(certificate => certificate.Id == id, ct);
    }

    public Task<CertificateEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken ct = default)
    {
        return CertificatesWithRelations
            .SingleOrDefaultAsync(
                certificate => certificate.Id == id && EF.Property<Guid>(certificate, "AdminId") == adminId,
                ct);
    }

    public async Task<CertificateEntity> AddAsync(CertificateEntity certificate, CancellationToken ct = default)
    {
        await dbContext.Certificates.AddAsync(certificate, ct);
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Entry(certificate).ReloadAsync(ct);

        return certificate;
    }

    public async Task<CertificateEntity?> UpdateAsync(CertificateEntity certificate, CancellationToken ct = default)
    {
        var exists = await dbContext.Certificates.AnyAsync(item => item.Id == certificate.Id, ct);
        if (!exists)
        {
            return null;
        }

        certificate.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Certificates.Update(certificate);
        await dbContext.SaveChangesAsync(ct);

        return certificate;
    }

    public async Task<CertificateEntity?> UpdateAsync(Guid adminId, CertificateEntity certificate, CancellationToken ct = default)
    {
        var exists = await dbContext.Certificates.AnyAsync(
            item => item.Id == certificate.Id && EF.Property<Guid>(item, "AdminId") == adminId,
            ct);
        if (!exists)
        {
            return null;
        }

        certificate.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Certificates.Update(certificate);
        await dbContext.SaveChangesAsync(ct);

        return certificate;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var certificate = await GetByIdAsync(id, ct);
        if (certificate is null)
        {
            return false;
        }

        dbContext.Certificates.Remove(certificate);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid adminId, int id, CancellationToken ct = default)
    {
        var certificate = await GetByIdAsync(adminId, id, ct);
        if (certificate is null)
        {
            return false;
        }

        dbContext.Certificates.Remove(certificate);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteByDomainAsync(Guid adminId, long nodeId, string domain, CancellationToken ct = default)
    {
        var certificate = await GetByDomainAsync(adminId, nodeId, domain, ct);
        if (certificate is null)
        {
            return false;
        }

        dbContext.Certificates.Remove(certificate);
        await dbContext.SaveChangesAsync(ct);

        return true;
    }
}
