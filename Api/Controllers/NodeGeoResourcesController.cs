using Api.Exceptions;
using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Values;
using Data.Entities;
using Infrastructure.Dto;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoteNode.Exceptions;

namespace Api.Controllers;

/// <summary>
/// Manages geo resources assigned to remote nodes.
/// </summary>
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/nodes/{nodeId:long}/geo-resources")]
public sealed class NodeGeoResourcesController(
    INodeService nodes,
    INodeGeoResourceService geoResources,
    IMapper mapper) : ApiControllerBase
{
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
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var result = await geoResources.SynchronizeNodeAsync(AdminId, node, cancellationToken);

            return mapper.Map<List<NodeGeoResourceDto>>(result);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
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
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);
        var result = await geoResources.GetAllAsync(AdminId, node, cancellationToken);

        return result.Select(resource => resource.Filename).Order(StringComparer.Ordinal).ToList();
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
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var content = await geoResources.DownloadAsync(AdminId, node, geoResourceId, cancellationToken);

            return File(content.Content, "application/octet-stream", content.FileName);
        }
        catch (NodeGeoResourceException exception)
        {
            throw ToApiException(exception);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
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
    public async Task<IActionResult> CreateFile(
        long nodeId,
        [FromForm] CreateNodeGeoResourceFileRequest request,
        CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);
        if (request.File is null || request.File.Length == 0)
        {
            throw new BadRequestException("Geo resource file is required.");
        }

        var fileName = string.IsNullOrWhiteSpace(request.FileName)
            ? request.File.FileName
            : request.FileName;

        try
        {
            await using var stream = request.File.OpenReadStream();
            var created = await geoResources.CreateFileAsync(
                AdminId,
                node,
                fileName,
                stream,
                cancellationToken);

            return Created($"/api/nodes/{nodeId}/geo-resources/{created.Id}", mapper.Map<NodeGeoResourceDto>(created));
        }
        catch (NodeGeoResourceException exception)
        {
            throw ToApiException(exception);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
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
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var created = await geoResources.CreateAutoUpdateAsync(
                AdminId,
                node,
                request.FileName,
                request.Url,
                request.CronTemplate,
                cancellationToken);

            return Created($"/api/nodes/{nodeId}/geo-resources/{created.Id}", mapper.Map<NodeGeoResourceDto>(created));
        }
        catch (NodeGeoResourceException exception)
        {
            throw ToApiException(exception);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw new BadRequestException(exception.Message);
        }
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
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            var updated = await geoResources.UpdateAsync(
                AdminId,
                node,
                geoResourceId,
                request.FileName,
                request.Url,
                request.CronTemplate,
                cancellationToken);

            return mapper.Map<NodeGeoResourceDto>(updated);
        }
        catch (NodeGeoResourceException exception)
        {
            throw ToApiException(exception);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
        catch (HttpRequestException exception)
        {
            throw new BadRequestException(exception.Message);
        }
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
        var node = await GetAccessibleNodeAsync(nodeId, cancellationToken);

        try
        {
            await geoResources.DeleteAsync(AdminId, node, geoResourceId, cancellationToken);

            return NoContent();
        }
        catch (NodeGeoResourceException exception)
        {
            throw ToApiException(exception);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
    }

    private async Task<NodeEntity> GetAccessibleNodeAsync(long nodeId, CancellationToken cancellationToken)
    {
        var node = await nodes.GetByIdAsync(AdminId, nodeId, cancellationToken);

        return node ?? throw new NotFoundException($"Node '{nodeId}' was not found.");
    }

}
