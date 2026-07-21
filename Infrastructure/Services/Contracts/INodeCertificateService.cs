using Data.Entities;
using Node.Models;

namespace Infrastructure.Services;

/// <summary>
/// Synchronizes remote node certificates with panel metadata.
/// </summary>
public interface INodeCertificateService
{
    /// <summary>
    /// Lists remote node certificates and synchronizes panel metadata.
    /// </summary>
    Task<List<CertificateEntity>> GetAllAsync(
        long adminId,
        NodeEntity node,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a certificate on the remote node and stores metadata.
    /// </summary>
    Task<CertificateEntity> IssueAsync(
        long adminId,
        NodeEntity node,
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports certificate files from remote node paths and stores metadata.
    /// </summary>
    Task<CertificateEntity> UploadAsync(
        long adminId,
        NodeEntity node,
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews a remote certificate and updates metadata.
    /// </summary>
    Task<CertificateEntity> RenewAsync(
        long adminId,
        NodeEntity node,
        string domain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes remote certificate files and local metadata.
    /// </summary>
    Task DeleteAsync(
        long adminId,
        NodeEntity node,
        string domain,
        CancellationToken cancellationToken = default);
}
