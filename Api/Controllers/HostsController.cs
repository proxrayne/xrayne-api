using Api.Mapping;
using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Exceptions;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Manages subscription hosts.
/// </summary>
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/hosts")]
public sealed class HostsController(IHostRepository hosts, IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets hosts available to the current administrator.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List hosts")]
    [EndpointDescription("Get all hosts owned by the current administrator in display order.")]
    [ProducesResponseType(typeof(List<HostListItemDto>), StatusCodes.Status200OK)]
    public async Task<List<HostListItemDto>> GetAll(CancellationToken cancellationToken)
    {
        var items = await hosts.GetAllAsync(cancellationToken);

        return mapper.Map<List<HostListItemDto>>(items);
    }

    /// <summary>
    /// Gets inbound options grouped by node.
    /// </summary>
    [HttpGet("inbounds")]
    [EndpointSummary("List host inbound options")]
    [EndpointDescription("Get inbounds grouped by node for host selection.")]
    [ProducesResponseType(typeof(List<HostNodeInboundsDto>), StatusCodes.Status200OK)]
    public async Task<List<HostNodeInboundsDto>> GetInbounds(
        [FromQuery] HostInboundOptionsQuery query,
        CancellationToken cancellationToken)
    {
        var inbounds = await hosts.GetInboundOptionsAsync(AdminId, query.Search, cancellationToken);

        var groups = inbounds
            .GroupBy(inbound => new { inbound.Node.Id, inbound.Node.Name })
            .OrderBy(group => group.Key.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => new HostInboundGroup(group.Key.Id, group.Key.Name, group.ToList()))
            .ToList();

        return mapper.Map<List<HostNodeInboundsDto>>(groups);
    }

    /// <summary>
    /// Gets one host by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get host")]
    [EndpointDescription("Get one host for editing.")]
    [ProducesResponseType(typeof(HostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<HostDto> GetById(long id, CancellationToken cancellationToken)
    {
        var host = await hosts.GetByIdAsync(id, cancellationToken);

        return mapper.Map<HostDto>(host);
    }

    /// <summary>
    /// Creates a host.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create host")]
    [EndpointDescription("Create a host and append it to the current administrator host order.")]
    [ProducesResponseType(typeof(HostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateHostRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureInboundExistsAsync(request.InboundId, cancellationToken);
        var created = await hosts.AddAsync(AdminId, mapper.Map<HostEntity>(request), cancellationToken);

        return Created($"/api/hosts/{created.Id}", mapper.Map<HostDto>(created));
    }

    /// <summary>
    /// Updates a host.
    /// </summary>
    [HttpPut("{id:long}")]
    [EndpointSummary("Update host")]
    [EndpointDescription("Replace all editable host fields.")]
    [ProducesResponseType(typeof(HostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<HostDto> Update(
        long id,
        [FromBody] UpdateHostRequest request,
        CancellationToken cancellationToken)
    {
        _ = await hosts.GetByIdAsync(id, cancellationToken);
        await EnsureInboundExistsAsync(request.InboundId, cancellationToken);

        var updated = await hosts.UpdateAsync(id, mapper.Map<HostEntity>(request), cancellationToken)
            ?? throw new NotFoundException($"Host '{id}' was not found.");

        return mapper.Map<HostDto>(updated);
    }

    /// <summary>
    /// Partially updates a host.
    /// </summary>
    [HttpPatch("{id:long}")]
    [EndpointSummary("Patch host")]
    [EndpointDescription("Partially update a host using only fields present in the request body.")]
    [ProducesResponseType(typeof(HostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<HostDto> Patch(
        long id,
        [FromBody] PatchHostRequest request,
        CancellationToken cancellationToken)
    {
        _ = await hosts.GetByIdAsync(id, cancellationToken);
        if (request.InboundId.IsSpecified)
        {
            await EnsureInboundExistsAsync(request.InboundId.SpecifiedValue, cancellationToken);
        }

        var updated = await hosts.PatchAsync(id, mapper.Map<HostPatch>(request), cancellationToken)
            ?? throw new NotFoundException($"Host '{id}' was not found.");

        return mapper.Map<HostDto>(updated);
    }

    /// <summary>
    /// Reorders hosts.
    /// </summary>
    [HttpPut("order")]
    [EndpointSummary("Reorder hosts")]
    [EndpointDescription("Set the complete display order for hosts owned by the current administrator.")]
    [ProducesResponseType(typeof(List<HostListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<List<HostListItemDto>> UpdateOrder(
        [FromBody] UpdateHostOrderRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await hosts.UpdateOrderAsync(request.HostIds, cancellationToken);

        return mapper.Map<List<HostListItemDto>>(updated);
    }

    /// <summary>
    /// Deletes a host.
    /// </summary>
    [HttpDelete("{id:long}")]
    [EndpointSummary("Delete host")]
    [EndpointDescription("Delete a host owned by the current administrator.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await hosts.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Host '{id}' was not found.");
        }

        return NoContent();
    }

    private async Task EnsureInboundExistsAsync(long inboundId, CancellationToken cancellationToken)
    {
        if (inboundId <= 0 || !await hosts.InboundExistsAsync(inboundId, cancellationToken))
        {
            throw new BadRequestException("Selected inbound was not found.");
        }
    }
}
