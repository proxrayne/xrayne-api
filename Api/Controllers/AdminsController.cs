using System.ComponentModel.DataAnnotations;
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
using Data.Models;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptionalValues;

namespace Api.Controllers;

/// <summary>
/// Manages administrator accounts.
/// </summary>
[Authorize(Policy = AdminPermissionNames.ManageAdmins)]
[Route("api/admin")]
public sealed class AdminsController(
    IAdminAccountRepository adminAccounts,
    IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets active administrator accounts.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List administrator accounts")]
    [EndpointDescription("Get active administrator accounts with search by username or email and offset pagination.")]
    [ProducesResponseType(typeof(PageResponse<AdminDto>), StatusCodes.Status200OK)]
    public async Task<PageResponse<AdminDto>> GetAll(
        [FromQuery] AdminListQuery query,
        CancellationToken cancellationToken)
    {
        var page = await adminAccounts.SearchAsync(
            new AdminFilter
            {
                Search = query.Search,
                Page = query.Page,
                Limit = query.Limit
            },
            cancellationToken);

        return new PageResponse<AdminDto>(
            mapper.Map<List<AdminDto>>(page.Items),
            page.TotalItems,
            page.CurrentPage,
            page.TotalPages);
    }

    /// <summary>
    /// Gets one administrator account by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get administrator account")]
    [EndpointDescription("Get one active administrator account for editing.")]
    [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<AdminDto> GetById(long id, CancellationToken cancellationToken)
    {
        var caller = await GetCurrentAdminAsync(cancellationToken);
        var account = await adminAccounts.GetActiveByIdAsync(id, cancellationToken);

        AdminManagementGuard.EnsureCanManage(caller.Permissions, account.Permissions);

        return mapper.Map<AdminDto>(account);
    }

    /// <summary>
    /// Creates an administrator account.
    /// </summary>
    [HttpPost("create")]
    [EndpointSummary("Create administrator account")]
    [EndpointDescription("Creates a new administrator account with a password and a comma-separated permission list.")]
    [ProducesResponseType(typeof(AdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdminRequest request,
        CancellationToken cancellationToken)
    {
        var caller = await GetCurrentAdminAsync(cancellationToken);
        var username = NormalizeUsername(request.Username);
        var email = NormalizeEmail(request.Email);
        ValidatePassword(request.Password, "Password is required.");
        var permissions = AdminPermissionNames.ParseMany(request.Permissions);
        AdminManagementGuard.EnsureCanManage(caller.Permissions, AdminPermission.None, permissions);

        if (await adminAccounts.ExistsAsync(username, cancellationToken))
        {
            throw new ConflictException($"Admin account '{username}' already exists.");
        }

        if (email is not null && await adminAccounts.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException($"Admin email '{email}' already exists.");
        }

        var account = new AdminAccountEntity
        {
            Username = username,
            Email = email,
            PasswordHash = IdentityPasswordHasher.HashPassword(request.Password),
            Permissions = permissions
        };

        await adminAccounts.AddAsync(account, cancellationToken);

        return Created($"/api/admin/{account.Id}", mapper.Map<AdminDto>(account));
    }

    /// <summary>
    /// Partially updates an administrator account.
    /// </summary>
    [HttpPatch("{id:long}")]
    [EndpointSummary("Patch administrator account")]
    [EndpointDescription("Partially update an administrator account using only fields present in the request body.")]
    [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<AdminDto> Patch(
        [FromRoute] long id,
        [FromBody] PatchAdminRequest request,
        CancellationToken cancellationToken)
    {
        var caller = await GetCurrentAdminAsync(cancellationToken);
        var target = await adminAccounts.GetActiveByIdAsync(id, cancellationToken);
        var requestedPermissions = request.Permissions.IsSpecified
            ? AdminPermissionNames.ParseMany(request.Permissions.SpecifiedValue ?? string.Empty)
            : (AdminPermission?)null;
        AdminManagementGuard.EnsureCanManage(caller.Permissions, target.Permissions, requestedPermissions);

        if (request.Username.IsSpecified)
        {
            var username = NormalizeUsername(request.Username.SpecifiedValue);
            if (await adminAccounts.ExistsAsync(username, id, cancellationToken))
            {
                throw new ConflictException($"Admin account '{username}' already exists.");
            }
        }

        if (request.Email.IsSpecified)
        {
            var email = NormalizeEmail(request.Email.SpecifiedValue);
            if (email is not null && await adminAccounts.EmailExistsAsync(email, id, cancellationToken))
            {
                throw new ConflictException($"Admin email '{email}' already exists.");
            }
        }

        var updated = await adminAccounts.UpdateAsync(
            id,
            new AdminAccountPatch
            {
                Username = request.Username.IsSpecified
                    ? NormalizeUsername(request.Username.SpecifiedValue)
                    : OptionalValue<string?>.Unspecified,
                Email = request.Email.IsSpecified
                    ? NormalizeEmail(request.Email.SpecifiedValue)
                    : OptionalValue<string?>.Unspecified,
                Permissions = requestedPermissions.HasValue
                    ? requestedPermissions.Value
                    : OptionalValue<AdminPermission>.Unspecified
            },
            cancellationToken);

        return mapper.Map<AdminDto>(updated);
    }

    /// <summary>
    /// Changes an administrator password.
    /// </summary>
    [HttpPut("{id:long}/password")]
    [EndpointSummary("Change administrator password")]
    [EndpointDescription("Changes the password for an existing administrator account. Super administrators do not need old-password confirmation.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        [FromRoute] long id,
        [FromBody] ChangeAdminPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var caller = await GetCurrentAdminAsync(cancellationToken);
        var target = await adminAccounts.GetActiveByIdAsync(id, cancellationToken);
        AdminManagementGuard.EnsureCanManage(caller.Permissions, target.Permissions);
        ValidatePassword(request.Password, "New password is required.");

        if (!caller.Permissions.HasFlag(AdminPermission.SuperAdmin))
        {
            ValidatePassword(request.OldPassword, "Old password is required.");
            if (!IdentityPasswordHasher.VerifyPassword(request.OldPassword!, caller.PasswordHash))
            {
                throw new BadRequestException("Old password is invalid.");
            }

            if (!string.Equals(request.Password, request.PasswordConfirmation, StringComparison.Ordinal))
            {
                throw new BadRequestException("Password confirmation does not match.");
            }
        }

        await adminAccounts.ChangePasswordAsync(
             id,
             IdentityPasswordHasher.HashPassword(request.Password),
             cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Replaces administrator permissions.
    /// </summary>
    [HttpPut("{id:long}/permissions")]
    [EndpointSummary("Change administrator permissions")]
    [EndpointDescription("Replaces the administrator permission set with a comma-separated permission list.")]
    [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePermissions(
        [FromRoute] long id,
        [FromBody] ChangeAdminPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var caller = await GetCurrentAdminAsync(cancellationToken);
        var target = await adminAccounts.GetActiveByIdAsync(id, cancellationToken);
        var permissions = AdminPermissionNames.ParseMany(request.Permissions);
        AdminManagementGuard.EnsureCanManage(caller.Permissions, target.Permissions, permissions);

        var account = await adminAccounts.ChangePermissionsAsync(
            id,
            permissions,
            cancellationToken);

        return Ok(mapper.Map<AdminDto>(account));
    }

    /// <summary>
    /// Soft deletes an administrator account.
    /// </summary>
    [HttpDelete("{id:long}")]
    [EndpointSummary("Delete administrator account")]
    [EndpointDescription("Soft deletes an administrator account.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        if (id == AdminId)
        {
            throw new BadRequestException("Cannot delete the current administrator account.");
        }

        var caller = await GetCurrentAdminAsync(cancellationToken);
        var target = await adminAccounts.GetActiveByIdAsync(id, cancellationToken);
        AdminManagementGuard.EnsureCanManage(caller.Permissions, target.Permissions);

        await adminAccounts.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    private async Task<AdminAccountEntity> GetCurrentAdminAsync(CancellationToken cancellationToken)
    {
        return await adminAccounts.GetActiveByIdAsync(AdminId, cancellationToken);
    }

    private static string NormalizeUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new BadRequestException("Username is required.");
        }

        return username.Trim();
    }

    private static void ValidatePassword(string? password, string message)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new BadRequestException(message);
        }
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalized = email.Trim();

        if (!new EmailAddressAttribute().IsValid(normalized))
        {
            throw new BadRequestException("Email address is invalid.");
        }

        return normalized;
    }
}
