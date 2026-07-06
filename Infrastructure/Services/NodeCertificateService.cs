using System.Net;
using Data.Contracts;
using Data.Entities;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using RemoteNode.Services;

namespace Infrastructure.Services;

/// <summary>
/// Synchronizes remote node certificate metadata with the panel database.
/// </summary>
public sealed class NodeCertificateService(
    ICertificateRepository certificates,
    INodeSecretService secrets,
    IRemoteNodeApiClientFactory apiClientFactory) : INodeCertificateService
{
    /// <inheritdoc />
    public async Task<List<CertificateEntity>> GetAllAsync(
        Guid adminId,
        NodeEntity node,
        CancellationToken cancellationToken = default)
    {
        var remoteCertificates = await CreateClient(node).GetCertificatesAsync(cancellationToken);

        return await SynchronizeAsync(adminId, node, remoteCertificates, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CertificateEntity> IssueAsync(
        Guid adminId,
        NodeEntity node,
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var remoteCertificate = await CreateClient(node).IssueCertificateAsync(request, cancellationToken);

        return await UpsertAsync(adminId, node, remoteCertificate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CertificateEntity> UploadAsync(
        Guid adminId,
        NodeEntity node,
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var remoteCertificate = await CreateClient(node).UploadCertificateAsync(request, cancellationToken);

        return await UpsertAsync(adminId, node, remoteCertificate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CertificateEntity> RenewAsync(
        Guid adminId,
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
        Guid adminId,
        NodeEntity node,
        string domain,
        CancellationToken cancellationToken = default)
    {
        var normalizedDomain = NormalizeDomain(domain);

        try
        {
            await CreateClient(node).DeleteCertificateAsync(normalizedDomain, cancellationToken);
        }
        catch (RemoteNodeHttpException exception) when (exception.StatusCode is HttpStatusCode.NotFound)
        {
        }

        _ = await certificates.DeleteByDomainAsync(adminId, node.Id, normalizedDomain, cancellationToken);
    }

    private async Task<List<CertificateEntity>> SynchronizeAsync(
        Guid adminId,
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
        Guid adminId,
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
                Node = node,
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

    private IRemoteNodeApiClient CreateClient(NodeEntity node)
    {
        var endpoint = new RemoteNodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey));

        return apiClientFactory.Create(endpoint);
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
