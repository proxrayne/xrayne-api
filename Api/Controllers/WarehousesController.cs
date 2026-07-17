using Api.Requests;
using Api.Responses;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Manages connection warehouses.
/// </summary>
[Authorize(Policy = AdminPermissionNames.ManageWarehouses)]
[Route("api/warehouses")]
public sealed class WarehousesController(IWarehouseRepository warehouses) : ApiControllerBase
{
    /// <summary>
    /// Gets warehouses available to the current administrator.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List warehouses")]
    [EndpointDescription("Get warehouses with filtering by name, enabled state, and assigned inbounds.")]
    [ProducesResponseType(typeof(PageResponse<WarehouseListItemDto>), StatusCodes.Status200OK)]
    public async Task<PageResponse<WarehouseListItemDto>> GetAll(
        [FromQuery] WarehouseListQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new WarehouseFilter
        {
            Search = query.Search,
            Enabled = query.Enabled,
            InboundIds = query.InboundIds,
            Page = query.Page,
            Limit = query.Limit
        };
        var page = await warehouses.SearchAsync(AdminId, filter, cancellationToken);

        return new PageResponse<WarehouseListItemDto>(
            page.Items.Select(ToListItemDto).ToList(),
            page.TotalItems,
            page.CurrentPage,
            page.TotalPages);
    }

    /// <summary>
    /// Gets inbound options grouped by node.
    /// </summary>
    [HttpGet("inbounds-by-node")]
    [EndpointSummary("List warehouse inbound options")]
    [EndpointDescription("Get inbounds grouped by node for warehouse selection.")]
    [ProducesResponseType(typeof(List<WarehouseNodeInboundsDto>), StatusCodes.Status200OK)]
    public async Task<List<WarehouseNodeInboundsDto>> GetInboundsByNode(
        [FromQuery] WarehouseInboundOptionsQuery query,
        CancellationToken cancellationToken)
    {
        var inbounds = await warehouses.GetInboundOptionsAsync(AdminId, query.Search, cancellationToken);

        return inbounds
            .GroupBy(inbound => new { inbound.Node.Id, inbound.Node.Name })
            .OrderBy(group => group.Key.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => new WarehouseNodeInboundsDto(
                group.Key.Id,
                group.Key.Name,
                group.Select(ToInboundDto).ToList()))
            .ToList();
    }

    /// <summary>
    /// Gets one warehouse by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get warehouse")]
    [EndpointDescription("Get one warehouse with its assigned inbounds.")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<WarehouseDto> GetById(long id, CancellationToken cancellationToken)
    {
        var warehouse = await GetAccessibleWarehouseAsync(id, cancellationToken);

        return ToDto(warehouse);
    }

    /// <summary>
    /// Creates a warehouse.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create warehouse")]
    [EndpointDescription("Create a warehouse and assign selected inbounds.")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        ValidateName(request.Name);
        var inbounds = await ResolveInboundsAsync(request.InboundIds, cancellationToken);
        var warehouse = new WarehouseEntity
        {
            Name = request.Name.Trim(),
            Note = request.Note.Trim(),
            Enabled = request.Enabled
        };

        var created = await warehouses.AddAsync(AdminId, warehouse, inbounds, cancellationToken);

        return Created($"/api/warehouses/{created.Id}", ToDto(created));
    }

    /// <summary>
    /// Updates a warehouse.
    /// </summary>
    [HttpPut("{id:long}")]
    [EndpointSummary("Update warehouse")]
    [EndpointDescription("Update a warehouse and replace its assigned inbounds.")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<WarehouseDto> Update(
        long id,
        [FromBody] UpdateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        ValidateName(request.Name);
        var inbounds = await ResolveInboundsAsync(request.InboundIds, cancellationToken);
        var updated = await warehouses.UpdateAsync(
            AdminId,
            id,
            new WarehouseEntity
            {
                Name = request.Name.Trim(),
                Note = request.Note.Trim(),
                Enabled = request.Enabled
            },
            inbounds,
            cancellationToken);

        if (updated is null)
        {
            throw new NotFoundException($"Warehouse '{id}' was not found.");
        }

        return ToDto(updated);
    }

    /// <summary>
    /// Deletes an empty warehouse.
    /// </summary>
    [HttpDelete("{id:long}")]
    [EndpointSummary("Delete warehouse")]
    [EndpointDescription("Delete a warehouse when no users are assigned to it.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        _ = await GetAccessibleWarehouseAsync(id, cancellationToken);
        if (await warehouses.HasUsersAsync(AdminId, id, cancellationToken))
        {
            throw new ConflictException("Warehouse cannot be deleted while users are assigned to it.");
        }

        await warehouses.DeleteAsync(AdminId, id, cancellationToken);

        return NoContent();
    }

    private async Task<WarehouseEntity> GetAccessibleWarehouseAsync(
        long id,
        CancellationToken cancellationToken)
    {
        return await warehouses.GetByIdAsync(AdminId, id, cancellationToken)
            ?? throw new NotFoundException($"Warehouse '{id}' was not found.");
    }

    private async Task<List<InboundEntity>> ResolveInboundsAsync(
        IReadOnlyCollection<long> inboundIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = inboundIds.Distinct().ToArray();
        var inbounds = await warehouses.GetInboundsByIdsAsync(AdminId, distinctIds, cancellationToken);
        if (inbounds.Count != distinctIds.Length)
        {
            throw new BadRequestException("One or more selected inbounds were not found.");
        }

        return inbounds;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Warehouse name is required.");
        }
    }

    private static WarehouseListItemDto ToListItemDto(WarehouseEntity warehouse)
    {
        return new WarehouseListItemDto(
            warehouse.Id,
            warehouse.Name,
            warehouse.Enabled,
            warehouse.Users.Count,
            warehouse.Inbounds.Count);
    }

    private static WarehouseDto ToDto(WarehouseEntity warehouse)
    {
        return new WarehouseDto(
            warehouse.Id,
            warehouse.Name,
            warehouse.Note,
            warehouse.Enabled,
            warehouse.Inbounds
                .OrderBy(inbound => inbound.Node.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(inbound => inbound.Tag, StringComparer.OrdinalIgnoreCase)
                .Select(ToInboundDto)
                .ToList(),
            warehouse.Users.Count);
    }

    private static WarehouseInboundDto ToInboundDto(InboundEntity inbound)
    {
        return new WarehouseInboundDto(
            inbound.Id,
            inbound.Tag,
            inbound.Port.ToString(),
            inbound.Protocol,
            inbound.Network,
            inbound.Security,
            inbound.Enabled,
            inbound.Node.Id,
            inbound.Node.Name);
    }
}
