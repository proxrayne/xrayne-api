using Node.Models;

namespace Node.Services;

/// <summary>
/// Sends authenticated certificate requests to one remote node.
/// </summary>
public interface INodeCertificateClient
{
    /// <summary>
    /// Gets certificates available on the remote node.
    /// </summary>
    Task<List<CertificateDto>> GetCertificatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a Let's Encrypt certificate on the remote node.
    /// </summary>
    Task<CertificateDto> IssueCertificateAsync(
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a certificate and private key to the remote node.
    /// </summary>
    Task<CertificateDto> UploadCertificateAsync(
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews an existing remote node certificate.
    /// </summary>
    Task<CertificateDto> RenewCertificateAsync(string domain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a remote node certificate directory.
    /// </summary>
    Task DeleteCertificateAsync(string domain, CancellationToken cancellationToken = default);
}
