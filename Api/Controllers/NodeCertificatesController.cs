using Api.Exceptions;
using Api.Requests;
using Api.Responses;
using Contracts.Values;
using Data.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoteNode.Exceptions;
using RemoteNode.Models;

namespace Api.Controllers;

/// <summary>
/// Manages certificates stored on remote nodes.
/// </summary>
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/nodes/{nodeId:long}/certificates")]
public sealed class NodeCertificatesController(
    INodeService nodes,
    INodeCertificateService certificates) : ApiControllerBase
{
    /// <summary>
    /// Lists certificates available on a remote node.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List node certificates")]
    [EndpointDescription("Synchronize and return certificates stored on a remote node.")]
    [ProducesResponseType(typeof(List<NodeCertificateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<List<NodeCertificateDto>> GetAll(long nodeId, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var result = await certificates.GetAllAsync(AdminId, node, cancellationToken);

            return result.Select(ToDto).ToList();
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Issues a Let's Encrypt certificate on a remote node.
    /// </summary>
    [HttpPost("issue")]
    [EndpointSummary("Issue node certificate")]
    [EndpointDescription("Issue a Let's Encrypt HTTP-01 certificate on a remote node and store panel metadata.")]
    [ProducesResponseType(typeof(NodeCertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<NodeCertificateDto> Issue(
        long nodeId,
        [FromBody] IssueNodeCertificateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Domain))
        {
            throw new BadRequestException("Certificate domain is required.");
        }

        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var result = await certificates.IssueAsync(
                AdminId,
                node,
                new IssueCertificateRequest(request.Domain),
                cancellationToken);

            return ToDto(result);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException(exception.Message);
        }
    }

    /// <summary>
    /// Imports certificate files from remote node paths.
    /// </summary>
    [HttpPost("upload")]
    [EndpointSummary("Upload node certificate")]
    [EndpointDescription("Import certificate PEM and private key PEM from remote node file paths without storing PEM content in the panel database.")]
    [ProducesResponseType(typeof(NodeCertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<NodeCertificateDto> Upload(
        long nodeId,
        [FromBody] UploadNodeCertificateRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Domain)
            || string.IsNullOrWhiteSpace(request.CertificateFile)
            || string.IsNullOrWhiteSpace(request.PrivateKeyFile))
        {
            throw new BadRequestException("Domain, certificate file path, and private key file path are required.");
        }

        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var result = await certificates.UploadAsync(
                AdminId,
                node,
                new UploadCertificateRequest(request.Domain, request.CertificateFile, request.PrivateKeyFile),
                cancellationToken);

            return ToDto(result);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException(exception.Message);
        }
    }

    /// <summary>
    /// Renews a remote node certificate.
    /// </summary>
    [HttpPost("{domain}/renew")]
    [EndpointSummary("Renew node certificate")]
    [EndpointDescription("Issue a replacement Let's Encrypt certificate for the same domain on a remote node.")]
    [ProducesResponseType(typeof(NodeCertificateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<NodeCertificateDto> Renew(
        long nodeId,
        string domain,
        CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var result = await certificates.RenewAsync(AdminId, node, domain, cancellationToken);

            return ToDto(result);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException(exception.Message);
        }
    }

    /// <summary>
    /// Deletes a certificate from a remote node.
    /// </summary>
    [HttpDelete("{domain}")]
    [EndpointSummary("Delete node certificate")]
    [EndpointDescription("Delete certificate files from a remote node and remove panel metadata.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(
        long nodeId,
        string domain,
        CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            await certificates.DeleteAsync(AdminId, node, domain, cancellationToken);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
        catch (ArgumentException exception)
        {
            throw new BadRequestException(exception.Message);
        }

        return NoContent();
    }

    private async Task<NodeEntity> GetAccessibleNodeAsync(long nodeId, CancellationToken cancellationToken)
    {
        var node = await nodes.GetByIdAsync(AdminId, nodeId, cancellationToken);

        return node ?? throw new NotFoundException($"Node '{nodeId}' was not found.");
    }

    private static NodeCertificateDto ToDto(CertificateEntity certificate)
    {
        return new NodeCertificateDto(
            certificate.Id,
            certificate.Domain,
            certificate.Active,
            certificate.ExpireAt,
            certificate.CertificateFile,
            certificate.PrivateKeyFile,
            certificate.CreatedAt,
            certificate.UpdatedAt);
    }

}
