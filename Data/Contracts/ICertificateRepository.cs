using Data.Entities;

namespace Data.Contracts;

public interface ICertificateRepository
{
    /// <summary>
    /// Gets all certificate metadata records.
    /// </summary>
    Task<List<CertificateEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all certificate metadata records owned by an administrator.
    /// </summary>
    Task<List<CertificateEntity>> GetAllAsync(long adminId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets certificate metadata records for one administrator and node.
    /// </summary>
    Task<List<CertificateEntity>> GetAllAsync(
        long adminId,
        long nodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one certificate metadata record by administrator, node, and normalized domain.
    /// </summary>
    Task<CertificateEntity?> GetByDomainAsync(
        long adminId,
        long nodeId,
        string domain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one certificate metadata record by id.
    /// </summary>
    Task<CertificateEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one certificate metadata record by administrator and id.
    /// </summary>
    Task<CertificateEntity?> GetByIdAsync(long adminId, int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a certificate metadata record.
    /// </summary>
    Task<CertificateEntity> AddAsync(CertificateEntity certificate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a certificate metadata record.
    /// </summary>
    Task<CertificateEntity?> UpdateAsync(CertificateEntity certificate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a certificate metadata record owned by an administrator.
    /// </summary>
    Task<CertificateEntity?> UpdateAsync(long adminId, CertificateEntity certificate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a certificate metadata record by id.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a certificate metadata record by administrator and id.
    /// </summary>
    Task<bool> DeleteAsync(long adminId, int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes one certificate metadata record by administrator, node, and normalized domain.
    /// </summary>
    Task<bool> DeleteByDomainAsync(
        long adminId,
        long nodeId,
        string domain,
        CancellationToken cancellationToken = default);
}
