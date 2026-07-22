using Api.Auth;
using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Exceptions;
using Contracts.Models;
using Data.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Manages user connections.
/// </summary>
[Authorize(Policy = AdminPermissionPolicies.ReadUsers)]
[Route("api/connection")]
public sealed class ConnectionController(
    IConnectionService connections,
    IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets connections assigned to a user.
    /// </summary>
    [HttpGet("user/{userId:long}")]
    [EndpointSummary("List user connections")]
    [EndpointDescription("Get connection metadata assigned to a subscription user with pagination, search, and revoked filtering.")]
    [ProducesResponseType(typeof(PageResponse<ConnectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<PageResponse<ConnectionDto>> GetByUserId(
        long userId,
        [FromQuery] ConnectionListQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new ConnectionFilter
        {
            Search = query.Search,
            IncludeRevoked = query.IncludeRevoked,
            Page = query.Page,
            Limit = query.Limit
        };
        var page = await connections.GetByUserIdAsync(userId, filter, cancellationToken);

        return new PageResponse<ConnectionDto>(
            mapper.Map<List<ConnectionDto>>(page.Items),
            page.TotalItems,
            page.CurrentPage,
            page.TotalPages);
    }

    /// <summary>
    /// Gets one connection by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get connection")]
    [EndpointDescription("Get one connection metadata record for editing.")]
    [ProducesResponseType(typeof(ConnectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ConnectionDto> GetById(long id, CancellationToken cancellationToken)
    {
        var connection = await connections.GetByIdAsync(id, cancellationToken);

        return mapper.Map<ConnectionDto>(connection);
    }

    /// <summary>
    /// Creates a connection.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create connection")]
    [EndpointDescription("Create a connection for a subscription user and generate its credentials.")]
    [ProducesResponseType(typeof(ConnectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await connections.CreateAsync(
            request.UserId,
            request.Name,
            request.Flow,
            request.Method,
            request.DeviceVerificationMethod,
            cancellationToken);

        return Created($"/api/connection/{created.Id}", mapper.Map<ConnectionDto>(created));
    }

    /// <summary>
    /// Updates a connection.
    /// </summary>
    [HttpPut("{id:long}")]
    [EndpointSummary("Update connection")]
    [EndpointDescription("Update editable connection metadata without changing generated credentials.")]
    [ProducesResponseType(typeof(ConnectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ConnectionDto> Update(
        long id,
        [FromBody] UpdateConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await connections.UpdateAsync(
            id,
            request.Name,
            request.Flow,
            request.Method,
            request.DeviceVerificationMethod,
            cancellationToken);

        return mapper.Map<ConnectionDto>(updated);
    }

    /// <summary>
    /// Partially updates a connection.
    /// </summary>
    [HttpPatch("{id:long}")]
    [EndpointSummary("Patch connection")]
    [EndpointDescription("Partially update editable connection metadata using only fields present in the request body.")]
    [ProducesResponseType(typeof(ConnectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ConnectionDto> Patch(
        long id,
        [FromBody] PatchConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await connections.PatchAsync(
            id,
            new ConnectionPatch
            {
                Name = NormalizeRequiredText(request.Name, "Connection name is required."),
                Flow = request.Flow,
                Method = request.Method,
                DeviceVerificationMethod = request.DeviceVerificationMethod
            },
            cancellationToken);

        return mapper.Map<ConnectionDto>(updated);
    }

    /// <summary>
    /// Revokes a connection.
    /// </summary>
    [HttpPost("{id:long}/revoke")]
    [EndpointSummary("Revoke connection")]
    [EndpointDescription("Revoke one user connection so its generated credentials can no longer be used.")]
    [ProducesResponseType(typeof(ConnectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ConnectionDto> Revoke(long id, CancellationToken cancellationToken)
    {
        var revoked = await connections.RevokeByIdAsync(id, cancellationToken);

        return mapper.Map<ConnectionDto>(revoked);
    }

    private static string NormalizeRequiredText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException(message);
        }

        return value.Trim();
    }
}
