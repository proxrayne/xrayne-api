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
/// Manages outbounds assigned to remote node profiles.
/// </summary>
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/nodes/{nodeId:long}/outbounds")]
public sealed class NodeOutboundsController(INodeOutboundService outbounds, IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets outbounds assigned to a remote node.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List node outbounds")]
    [EndpointDescription("Get outbounds assigned to a remote node profile.")]
    [ProducesResponseType(typeof(List<NodeOutboundListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<List<NodeOutboundListItemDto>> GetAll(long nodeId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await outbounds.GetByNodeIdAsync(nodeId, cancellationToken);

            return mapper.Map<List<NodeOutboundListItemDto>>(result);
        }
        catch (NodeOutboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Gets one outbound assigned to a remote node.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get node outbound")]
    [EndpointDescription("Get one outbound assigned to a remote node profile with its full JSON configuration.")]
    [ProducesResponseType(typeof(NodeOutboundDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeOutboundDto> GetById(
        long nodeId,
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var outbound = await outbounds.GetByNodeAndIdAsync(nodeId, id, cancellationToken);

            return mapper.Map<NodeOutboundDto>(outbound);
        }
        catch (NodeOutboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Creates a manually managed outbound for a remote node.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create node outbound")]
    [EndpointDescription("Create a manually managed outbound for a remote node profile.")]
    [ProducesResponseType(typeof(NodeOutboundDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        long nodeId,
        [FromBody] CreateNodeOutboundRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await outbounds.CreateAsync(
                AdminId,
                nodeId,
                request.Config,
                request.Enabled,
                cancellationToken);

            return Created($"/api/nodes/{nodeId}/outbounds/{created.Id}", mapper.Map<NodeOutboundDto>(created));
        }
        catch (NodeOutboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Updates a manually managed outbound for a remote node.
    /// </summary>
    [HttpPut("{id:long}")]
    [EndpointSummary("Update node outbound")]
    [EndpointDescription("Update a manually managed outbound assigned to a remote node profile.")]
    [ProducesResponseType(typeof(NodeOutboundDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NodeOutboundDto> Update(
        long nodeId,
        long id,
        [FromBody] UpdateNodeOutboundRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await outbounds.UpdateAsync(
                nodeId,
                id,
                request.Config,
                request.Enabled,
                cancellationToken);

            return mapper.Map<NodeOutboundDto>(updated);
        }
        catch (NodeOutboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Updates enabled state for an outbound assigned to a remote node.
    /// </summary>
    [HttpPatch("{id:long}/enabled")]
    [EndpointSummary("Toggle node outbound")]
    [EndpointDescription("Enable or disable an outbound assigned to a remote node profile.")]
    [ProducesResponseType(typeof(NodeOutboundDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NodeOutboundDto> UpdateEnabled(
        long nodeId,
        long id,
        [FromBody] UpdateNodeOutboundEnabledRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await outbounds.UpdateEnabledAsync(
                nodeId,
                id,
                request.Enabled,
                cancellationToken);

            return mapper.Map<NodeOutboundDto>(updated);
        }
        catch (NodeOutboundException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Deletes a manually managed outbound from a remote node.
    /// </summary>
    [HttpDelete("{id:long}")]
    [EndpointSummary("Delete node outbound")]
    [EndpointDescription("Delete a manually managed outbound assigned to a remote node profile.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long nodeId, long id, CancellationToken cancellationToken)
    {
        try
        {
            await outbounds.DeleteAsync(nodeId, id, cancellationToken);

            return NoContent();
        }
        catch (NodeOutboundException exception)
        {
            throw ToApiException(exception);
        }
    }
}
