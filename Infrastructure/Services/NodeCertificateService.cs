using System.Net;
using Data.Contracts;
using Data.Entities;
using Node.Exceptions;
using Node.Models;
using Node.Services;

namespace Infrastructure.Services;

/// <summary>
/// Synchronizes remote node certificate metadata with the panel database.
/// </summary>
public sealed class NodeCertificateService(
    ICertificateRepository certificates,
    INodeSecretService secrets,
    INodeCertificateClientFactory certificateClientFactory) : INodeCertificateService
{
    /// <inheritdoc />
    public async Task<List<CertificateEntity>> GetAllAsync(
        long adminId,
        NodeEntity node,
        CancellationToken cancellationToken = default)
    {
        var remoteCertificates = await CreateClient(node).GetCertificatesAsync(cancellationToken);

        return await SynchronizeAsync(adminId, node, remoteCertificates, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CertificateEntity> IssueAsync(
        long adminId,
        NodeEntity node,
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var remoteCertificate = await CreateClient(node).IssueCertificateAsync(request, cancellationToken);

        return await UpsertAsync(adminId, node, remoteCertificate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CertificateEntity> UploadAsync(
        long adminId,
        NodeEntity node,
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var remoteCertificate = await CreateClient(node).UploadCertificateAsync(request, cancellationToken);

        return await UpsertAsync(adminId, node, remoteCertificate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CertificateEntity> RenewAsync(
        long adminId,
        NodeEntity node,
        string domain,
        CancellationToken cancellationToken = default)
    {
        var remoteCertificate = await CreateClient(node).RenewCertificateAsync(
            NormalizeDomain(domain),
            cancellationToken);

        return await UpsertAsync(adminId, node, remoteCertificate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        long adminId,
        NodeEntity node,
        string domain,
        CancellationToken cancellationToken = default)
    {
        var normalizedDomain = NormalizeDomain(domain);

        try
        {
            await CreateClient(node).DeleteCertificateAsync(normalizedDomain, cancellationToken);
        }
        catch (NodeHttpException exception) when (exception.StatusCode is HttpStatusCode.NotFound)
        {
        }

        _ = await certificates.DeleteByDomainAsync(adminId, node.Id, normalizedDomain, cancellationToken);
    }

    private async Task<List<CertificateEntity>> SynchronizeAsync(
        long adminId,
        NodeEntity node,
        List<CertificateDto> remoteCertificates,
        CancellationToken cancellationToken)
    {
        var existing = await certificates.GetAllAsync(adminId, node.Id, cancellationToken);
        var remoteDomains = remoteCertificates
            .Select(certificate => NormalizeDomain(certificate.Domain))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var certificate in existing.Where(certificate => !remoteDomains.Contains(NormalizeDomain(certificate.Domain))))
        {
            _ = await certificates.DeleteAsync(adminId, certificate.Id, cancellationToken);
        }

        foreach (var remoteCertificate in remoteCertificates)
        {
            _ = await UpsertAsync(adminId, node, remoteCertificate, cancellationToken);
        }

        return await certificates.GetAllAsync(adminId, node.Id, cancellationToken);
    }

    private async Task<CertificateEntity> UpsertAsync(
        long adminId,
        NodeEntity node,
        CertificateDto remoteCertificate,
        CancellationToken cancellationToken)
    {
        var normalizedDomain = NormalizeDomain(remoteCertificate.Domain);
        var certificate = await certificates.GetByDomainAsync(adminId, node.Id, normalizedDomain, cancellationToken);

        if (certificate is null)
        {
            certificate = new CertificateEntity
            {
                Domain = normalizedDomain,
                Active = remoteCertificate.Active,
                ExpireAt = remoteCertificate.ExpireAt.UtcDateTime,
                CertificateFile = remoteCertificate.CertificateFile,
                PrivateKeyFile = remoteCertificate.PrivateKeyFile,
                NodeId = node.Id,
                Node = node,
                AdminId = adminId,
                Admin = node.Admin
            };

            return await certificates.AddAsync(certificate, cancellationToken);
        }

        certificate.Domain = normalizedDomain;
        certificate.Active = remoteCertificate.Active;
        certificate.ExpireAt = remoteCertificate.ExpireAt.UtcDateTime;
        certificate.CertificateFile = remoteCertificate.CertificateFile;
        certificate.PrivateKeyFile = remoteCertificate.PrivateKeyFile;

        return await certificates.UpdateAsync(adminId, certificate, cancellationToken)
            ?? certificate;
    }

    private INodeCertificateClient CreateClient(NodeEntity node)
    {
        var endpoint = new NodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey));

        return certificateClientFactory.Create(endpoint);
    }

    private static string NormalizeDomain(string domain)
    {
        var normalized = domain.Trim().TrimEnd('.').ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("Certificate domain is required.", nameof(domain));
        }

        return normalized;
    }
}
