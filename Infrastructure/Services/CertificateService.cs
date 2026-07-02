using XRayne.Repositories.Contracts;
using XRayne.Repositories.Entities;

namespace XRayne.Infrastructure.Services;

public sealed class CertificateService(ICertificateRepository repository) : ICertificateService
{
    public Task<List<CertificateEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);

    public Task<List<CertificateEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default)
        => repository.GetAllAsync(adminId, cancellationToken);

    public Task<CertificateEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(id, cancellationToken);

    public Task<CertificateEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken cancellationToken = default)
        => repository.GetByIdAsync(adminId, id, cancellationToken);

    public Task<CertificateEntity> AddAsync(CertificateEntity certificate, CancellationToken cancellationToken = default)
        => repository.AddAsync(certificate, cancellationToken);

    public Task<CertificateEntity?> UpdateAsync(CertificateEntity certificate, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(certificate, cancellationToken);

    public Task<CertificateEntity?> UpdateAsync(Guid adminId, CertificateEntity certificate, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(adminId, certificate, cancellationToken);

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(id, cancellationToken);

    public Task<bool> DeleteAsync(Guid adminId, int id, CancellationToken cancellationToken = default)
        => repository.DeleteAsync(adminId, id, cancellationToken);
}
