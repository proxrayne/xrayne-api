using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Values;
using Data.Contracts;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Api.Controllers;

/// <summary>
/// Manages geo resources assigned to remote nodes.
/// </summary>
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/nodes/{nodeId:long}/geo-resources")]
public sealed class NodeGeoResourcesController(
    INodeGeoResourceService geoResourcesService,
    IGeoResourceRepository geoResources,
    INodeRepository nodeRepository,
    IMapper mapper) : ApiControllerBase
{
    private const long GeoResourceUploadMaxSizeBytes = 128L * 1024 * 1024;
    private const string GeoResourceUploadMaxSizeLabel = "128 MB";

    /// <summary>
    /// Lists geo resources assigned to a remote node.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List node geo resources")]
    [EndpointDescription("Synchronize and return geo resources stored on a remote node.")]
    [ProducesResponseType(typeof(List<NodeGeoResourceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<List<NodeGeoResourceDto>> GetAll(long nodeId, CancellationToken cancellationToken)
    {
        var result = await geoResources.GetAllByNodeIdAsync(nodeId, cancellationToken);

        return mapper.Map<List<NodeGeoResourceDto>>(result);
    }

    /// <summary>
    /// Gets geo resource file name hints for routing rule editors.
    /// </summary>
    [HttpGet("names")]
    [EndpointSummary("List node geo resource names")]
    [EndpointDescription("Get geo resource file names stored for routing rule schema suggestions.")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<List<string>> GetNames(long nodeId, CancellationToken cancellationToken)
    {
        var result = await geoResources.GetAllAsync(AdminId, nodeId, cancellationToken);

        return result
            .Where(resource => resource.Status == GeoResourceStatus.Success)
            .Select(resource => resource.Filename)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Downloads a geo resource through the panel API.
    /// </summary>
    [HttpGet("{geoResourceId:long}/download")]
    [EndpointSummary("Download node geo resource")]
    [EndpointDescription("Download a geo resource file from a remote node through the panel API.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Download(
        long nodeId,
        long geoResourceId,
        CancellationToken cancellationToken)
    {
        var node = await nodeRepository.GetByIdAsync(nodeId, cancellationToken);
        var content = await geoResourcesService.DownloadResourceAsync(node, geoResourceId, cancellationToken);

        Response.ContentType = "application/octet-stream";
        Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = content.FileName
        }.ToString();

        await foreach (var chunk in content.Content.WithCancellation(cancellationToken))
        {
            await Response.Body.WriteAsync(chunk, cancellationToken);
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Creates a static geo resource from an uploaded file.
    /// </summary>
    [HttpPost("file")]
    [EndpointSummary("Create node geo resource file")]
    [EndpointDescription("Upload a static geo resource file to a remote node and store panel metadata.")]
    [ProducesResponseType(typeof(NodeGeoResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status413PayloadTooLarge)]
    [RequestSizeLimit(GeoResourceUploadMaxSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = GeoResourceUploadMaxSizeBytes)]
    public async Task<IActionResult> CreateFile(
        long nodeId,
        [FromForm] CreateNodeGeoResourceFileRequest request,
        CancellationToken cancellationToken)
    {
        var node = await nodeRepository.GetByIdAsync(nodeId, cancellationToken);
        if (request.File is null || request.File.Length == 0)
        {
            throw new BadRequestException("Geo resource file is required.");
        }

        if (request.File.Length > GeoResourceUploadMaxSizeBytes)
        {
            throw new BadRequestException($"Geo resource file must be {GeoResourceUploadMaxSizeLabel} or smaller.");
        }

        var fileName = string.IsNullOrWhiteSpace(request.FileName)
            ? request.File.FileName
            : request.FileName;

        await using var stream = request.File.OpenReadStream();
        var created = await geoResourcesService.CreateFileAsync(
            AdminId,
            node,
            fileName,
            stream,
            cancellationToken);

        return Created($"/api/nodes/{nodeId}/geo-resources/{created.Id}", mapper.Map<NodeGeoResourceDto>(created));
    }

    /// <summary>
    /// Creates an auto-updated geo resource.
    /// </summary>
    [HttpPost("auto-update")]
    [EndpointSummary("Create auto-updated node geo resource")]
    [EndpointDescription("Download a geo resource in the panel, upload it to a remote node, and schedule auto-update metadata.")]
    [ProducesResponseType(typeof(NodeGeoResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAutoUpdate(
        long nodeId,
        [FromBody] CreateNodeGeoResourceAutoUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var node = await nodeRepository.GetByIdAsync(nodeId, cancellationToken);
        var created = await geoResourcesService.CreateAutoUpdateAsync(
            AdminId,
            node,
            request.FileName,
            request.Url,
            request.UpdateInterval,
            cancellationToken);

        return Created($"/api/nodes/{nodeId}/geo-resources/{created.Id}", mapper.Map<NodeGeoResourceDto>(created));
    }

    /// <summary>
    /// Updates a geo resource.
    /// </summary>
    [HttpPut("{geoResourceId:long}")]
    [EndpointSummary("Update node geo resource")]
    [EndpointDescription("Rename a static geo resource or update auto-update metadata for an auto-updated geo resource.")]
    [ProducesResponseType(typeof(NodeGeoResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NodeGeoResourceDto> Update(
        long nodeId,
        long geoResourceId,
        [FromBody] UpdateNodeGeoResourceRequest request,
        CancellationToken cancellationToken)
    {
        var node = await nodeRepository.GetByIdAsync(nodeId, cancellationToken);
        var updated = await geoResourcesService.UpdateAsync(
            node,
            geoResourceId,
            request.FileName,
            request.Url,
            request.UpdateInterval,
            cancellationToken);

        return mapper.Map<NodeGeoResourceDto>(updated);
    }

    /// <summary>
    /// Deletes a geo resource.
    /// </summary>
    [HttpDelete("{geoResourceId:long}")]
    [EndpointSummary("Delete node geo resource")]
    [EndpointDescription("Delete a geo resource from a remote node and remove panel metadata.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        long nodeId,
        long geoResourceId,
        CancellationToken cancellationToken)
    {
        var node = await nodeRepository.GetByIdAsync(nodeId, cancellationToken);

        await geoResourcesService.DeleteAsync(node, geoResourceId, cancellationToken);

        return NoContent();
    }
}
