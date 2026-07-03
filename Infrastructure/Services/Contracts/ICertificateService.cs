using Repositories.Entities;

namespace Infrastructure.Services;

public interface ICertificateService
{
    Task<List<CertificateEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<CertificateEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<CertificateEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CertificateEntity?> GetByIdAsync(Guid adminId, int id, CancellationToken cancellationToken = default);

    Task<CertificateEntity> AddAsync(CertificateEntity certificate, CancellationToken cancellationToken = default);

    Task<CertificateEntity?> UpdateAsync(CertificateEntity certificate, CancellationToken cancellationToken = default);

    Task<CertificateEntity?> UpdateAsync(Guid adminId, CertificateEntity certificate, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, int id, CancellationToken cancellationToken = default);
}
