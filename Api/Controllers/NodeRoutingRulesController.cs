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
/// Manages routing rules assigned to remote node profiles.
/// </summary>
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/nodes/{nodeId:long}/routing-rules")]
public sealed class NodeRoutingRulesController(INodeRoutingRuleService routingRules, IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets routing rules assigned to a remote node.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List node routing rules")]
    [EndpointDescription("Get routing rules assigned to a remote node profile.")]
    [ProducesResponseType(typeof(List<NodeRoutingRuleListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<List<NodeRoutingRuleListItemDto>> GetAll(long nodeId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await routingRules.GetByNodeIdAsync(nodeId, cancellationToken);

            return mapper.Map<List<NodeRoutingRuleListItemDto>>(result);
        }
        catch (NodeRoutingRuleException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Gets one routing rule assigned to a remote node.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get node routing rule")]
    [EndpointDescription("Get one routing rule assigned to a remote node profile with its full JSON configuration.")]
    [ProducesResponseType(typeof(NodeRoutingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeRoutingRuleDto> GetById(
        long nodeId,
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var routingRule = await routingRules.GetByNodeAndIdAsync(nodeId, id, cancellationToken);

            return mapper.Map<NodeRoutingRuleDto>(routingRule);
        }
        catch (NodeRoutingRuleException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Creates a manually managed routing rule for a remote node.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create node routing rule")]
    [EndpointDescription("Create a manually managed routing rule for a remote node profile.")]
    [ProducesResponseType(typeof(NodeRoutingRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        long nodeId,
        [FromBody] CreateNodeRoutingRuleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await routingRules.CreateAsync(
                AdminId,
                nodeId,
                request.Config,
                request.Enabled,
                cancellationToken);

            return Created($"/api/nodes/{nodeId}/routing-rules/{created.Id}", mapper.Map<NodeRoutingRuleDto>(created));
        }
        catch (NodeRoutingRuleException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Updates a manually managed routing rule for a remote node.
    /// </summary>
    [HttpPut("{id:long}")]
    [EndpointSummary("Update node routing rule")]
    [EndpointDescription("Update a manually managed routing rule assigned to a remote node profile.")]
    [ProducesResponseType(typeof(NodeRoutingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeRoutingRuleDto> Update(
        long nodeId,
        long id,
        [FromBody] UpdateNodeRoutingRuleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await routingRules.UpdateAsync(
                nodeId,
                id,
                request.Config,
                request.Enabled,
                cancellationToken);

            return mapper.Map<NodeRoutingRuleDto>(updated);
        }
        catch (NodeRoutingRuleException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Updates enabled state for a routing rule assigned to a remote node.
    /// </summary>
    [HttpPatch("{id:long}/enabled")]
    [EndpointSummary("Toggle node routing rule")]
    [EndpointDescription("Enable or disable a routing rule assigned to a remote node profile.")]
    [ProducesResponseType(typeof(NodeRoutingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeRoutingRuleDto> UpdateEnabled(
        long nodeId,
        long id,
        [FromBody] UpdateNodeRoutingRuleEnabledRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await routingRules.UpdateEnabledAsync(
                nodeId,
                id,
                request.Enabled,
                cancellationToken);

            return mapper.Map<NodeRoutingRuleDto>(updated);
        }
        catch (NodeRoutingRuleException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Reorders manually managed routing rules for a remote node.
    /// </summary>
    [HttpPut("order")]
    [EndpointSummary("Reorder node routing rules")]
    [EndpointDescription("Set the order of manually managed routing rules assigned to a remote node profile.")]
    [ProducesResponseType(typeof(List<NodeRoutingRuleListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<List<NodeRoutingRuleListItemDto>> UpdateOrder(
        long nodeId,
        [FromBody] UpdateNodeRoutingRuleOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await routingRules.UpdateOrderAsync(
                nodeId,
                request.RuleIds,
                cancellationToken);

            return mapper.Map<List<NodeRoutingRuleListItemDto>>(updated);
        }
        catch (NodeRoutingRuleException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Deletes a manually managed routing rule from a remote node.
    /// </summary>
    [HttpDelete("{id:long}")]
    [EndpointSummary("Delete node routing rule")]
    [EndpointDescription("Delete a manually managed routing rule assigned to a remote node profile.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long nodeId, long id, CancellationToken cancellationToken)
    {
        try
        {
            await routingRules.DeleteAsync(nodeId, id, cancellationToken);

            return NoContent();
        }
        catch (NodeRoutingRuleException exception)
        {
            throw ToApiException(exception);
        }
    }
}
