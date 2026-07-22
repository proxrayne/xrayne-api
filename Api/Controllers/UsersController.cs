using Api.Auth;
using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Manages subscription users.
/// </summary>
[Authorize(Policy = AdminPermissionPolicies.ReadUsers)]
[Route("api/users")]
public sealed class UsersController(
    IUserRepository users,
    IWarehouseRepository warehouses,
    IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets users available to administrators with user permissions.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List users")]
    [EndpointDescription("Get users with filtering by username and status, offset pagination, and column sorting.")]
    [ProducesResponseType(typeof(PageResponse<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<PageResponse<UserListItemDto>> GetAll(
        [FromQuery] UserListQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new UserFilter
        {
            Search = query.Search,
            Status = query.Statuses,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder,
            Page = query.Page,
            Limit = query.Limit
        };
        var page = await users.SearchAsync(filter, cancellationToken);

        return new PageResponse<UserListItemDto>(
            mapper.Map<List<UserListItemDto>>(page.Items),
            page.TotalItems,
            page.CurrentPage,
            page.TotalPages);
    }

    /// <summary>
    /// Gets warehouse options for assigning users.
    /// </summary>
    [HttpGet("warehouse-options")]
    [EndpointSummary("List user warehouse options")]
    [EndpointDescription("Get warehouses that can be assigned to users.")]
    [ProducesResponseType(typeof(List<UserWarehouseOptionDto>), StatusCodes.Status200OK)]
    public async Task<List<UserWarehouseOptionDto>> GetWarehouseOptions(CancellationToken cancellationToken)
    {
        var page = await warehouses.SearchAsync(
            AdminId,
            new WarehouseFilter { Limit = 100 },
            cancellationToken);

        return mapper.Map<List<UserWarehouseOptionDto>>(page.Items);
    }

    /// <summary>
    /// Gets one user by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get user")]
    [EndpointDescription("Get one subscription user for editing.")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<UserDto> GetById(long id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);

        return mapper.Map<UserDto>(user);
    }

    /// <summary>
    /// Creates a user.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AdminPermissionNames.CreateUsers)]
    [EndpointSummary("Create user")]
    [EndpointDescription("Create a subscription user and assign it to a warehouse.")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var username = NormalizeUsername(request.Username);
        ValidateConnectionLimit(request.ConnectionLimit);
        ValidateOnHoldDays(request.OnHoldDays);

        if (await users.ExistsAsync(username, cancellationToken))
        {
            throw new ConflictException($"User '{username}' already exists.");
        }

        var warehouse = await ResolveWarehouseAsync(request.WarehouseId, cancellationToken);
        var status = UserStatus.Active;
        DateTimeOffset? onHoldExpire = null;
        var expireAt = request.ExpireAt;

        if (request.OnHoldDays is > 0)
        {
            status = UserStatus.OnHold;
            onHoldExpire = DateTimeOffset.UtcNow.AddDays(request.OnHoldDays.Value);
            expireAt = request.ExpireAt?.AddDays(request.OnHoldDays.Value);
        }

        var user = new UserEntity
        {
            Username = username,
            Note = request.Note.Trim(),
            DataLimit = request.DataLimitBytes,
            ConnectionLimit = request.ConnectionLimit,
            Status = status,
            LimitResetStrategy = expireAt is not null ? request.LimitResetStrategy : null,
            ExpireAt = expireAt,
            OnHoldExpire = onHoldExpire,
            Warehouse = warehouse
        };

        var created = await users.AddAsync(AdminId, user, cancellationToken);

        return Created($"/api/users/{created.Id}", mapper.Map<UserDto>(created));
    }

    /// <summary>
    /// Updates a user.
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Policy = AdminPermissionNames.EditUsers)]
    [EndpointSummary("Update user")]
    [EndpointDescription("Update user limits, status, note, expiration, and warehouse assignment.")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<UserDto> Update(
        long id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        ValidateConnectionLimit(request.ConnectionLimit);

        var warehouse = await ResolveWarehouseAsync(request.WarehouseId, cancellationToken);
        var updated = await users.UpdateAsync(
            id,
            new UserEntity
            {
                Username = string.Empty,
                Note = request.Note.Trim(),
                DataLimit = request.DataLimitBytes,
                ConnectionLimit = request.ConnectionLimit,
                Status = request.Disabled ? UserStatus.Disabled : UserStatus.Active,
                LimitResetStrategy = request.ExpireAt is not null ? request.LimitResetStrategy : null,
                ExpireAt = request.ExpireAt,
                OnHoldExpire = null
            },
            warehouse,
            cancellationToken);

        return mapper.Map<UserDto>(updated);
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = AdminPermissionNames.DeleteUsers)]
    [EndpointSummary("Delete user")]
    [EndpointDescription("Delete a subscription user and its connection records.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await users.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"User '{id}' was not found.");
        }

        return NoContent();
    }

    private async Task<WarehouseEntity> ResolveWarehouseAsync(
        long warehouseId,
        CancellationToken cancellationToken)
    {
        if (warehouseId <= 0)
        {
            throw new BadRequestException("Warehouse is required.");
        }

        return await warehouses.GetByIdAsync(AdminId, warehouseId, cancellationToken)
            ?? throw new BadRequestException("Selected warehouse was not found.");
    }

    private static string NormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new BadRequestException("Username is required.");
        }

        return username.Trim();
    }

    private static void ValidateConnectionLimit(uint connectionLimit)
    {
        if (connectionLimit == 0)
        {
            throw new BadRequestException("Connection limit must be greater than zero.");
        }
    }

    private static void ValidateOnHoldDays(int? onHoldDays)
    {
        if (onHoldDays < 0)
        {
            throw new BadRequestException("On-hold duration must be zero or greater.");
        }
    }
}
