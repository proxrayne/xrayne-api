using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Values;
using Infrastructure.Dto;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Manages inbounds assigned to remote node profiles.
/// </summary>
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/nodes/{nodeId:long}/inbounds")]
public sealed class NodeInboundsController(INodeInboundService inbounds, IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets inbounds assigned to a remote node.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List node inbounds")]
    [EndpointDescription("Get inbounds assigned to a remote node profile.")]
    [ProducesResponseType(typeof(List<NodeInboundListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<List<NodeInboundListItemDto>> GetAll(long nodeId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await inbounds.GetByNodeIdAsync(nodeId, cancellationToken);

            return mapper.Map<List<NodeInboundListItemDto>>(result);
        }
        catch (NodeInboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Gets one inbound assigned to a remote node.
    /// </summary>
    [HttpGet("{inboundId:long}")]
    [EndpointSummary("Get node inbound")]
    [EndpointDescription("Get one inbound assigned to a remote node profile with its full JSON configuration.")]
    [ProducesResponseType(typeof(NodeInboundDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeInboundDto> GetById(
        long nodeId,
        long inboundId,
        CancellationToken cancellationToken)
    {
        try
        {
            var inbound = await inbounds.GetByNodeAndIdAsync(nodeId, inboundId, cancellationToken);

            return mapper.Map<NodeInboundDto>(inbound);
        }
        catch (NodeInboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Creates a manually managed inbound for a remote node.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create node inbound")]
    [EndpointDescription("Create a manually managed inbound for a remote node profile.")]
    [ProducesResponseType(typeof(NodeInboundDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        long nodeId,
        [FromBody] CreateNodeInboundRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await inbounds.CreateAsync(
                AdminId,
                nodeId,
                request.Config,
                request.Enabled,
                cancellationToken);

            return Created($"/api/nodes/{nodeId}/inbounds/{created.Id}", mapper.Map<NodeInboundDto>(created));
        }
        catch (NodeInboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Updates a manually managed inbound for a remote node.
    /// </summary>
    [HttpPut("{inboundId:long}")]
    [EndpointSummary("Update node inbound")]
    [EndpointDescription("Update a manually managed inbound assigned to a remote node profile.")]
    [ProducesResponseType(typeof(NodeInboundDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NodeInboundDto> Update(
        long nodeId,
        long inboundId,
        [FromBody] UpdateNodeInboundRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await inbounds.UpdateAsync(
                nodeId,
                inboundId,
                request.Config,
                request.Enabled,
                cancellationToken);

            return mapper.Map<NodeInboundDto>(updated);
        }
        catch (NodeInboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Updates enabled state for an inbound assigned to a remote node.
    /// </summary>
    [HttpPatch("{inboundId:long}/enabled")]
    [EndpointSummary("Toggle node inbound")]
    [EndpointDescription("Enable or disable an inbound assigned to a remote node profile.")]
    [ProducesResponseType(typeof(NodeInboundDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NodeInboundDto> UpdateEnabled(
        long nodeId,
        long inboundId,
        [FromBody] UpdateNodeInboundEnabledRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await inbounds.UpdateEnabledAsync(
                nodeId,
                inboundId,
                request.Enabled,
                cancellationToken);

            return mapper.Map<NodeInboundDto>(updated);
        }
        catch (NodeInboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Deletes a manually managed inbound from a remote node.
    /// </summary>
    [HttpDelete("{inboundId:long}")]
    [EndpointSummary("Delete node inbound")]
    [EndpointDescription("Delete a manually managed inbound assigned to a remote node profile.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long nodeId, long inboundId, CancellationToken cancellationToken)
    {
        try
        {
            await inbounds.DeleteAsync(nodeId, inboundId, cancellationToken);

            return NoContent();
        }
        catch (NodeInboundException exception)
        {
            throw ToApiException(exception);
        }
    }

}
