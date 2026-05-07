using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRayne.Api.Exceptions;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Utilities;
using XRayne.Repositories.Admins;
using XRayne.Repositories.Entities;

namespace XRayne.Api.Controllers;

[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Route("api/admins")]
public sealed class AdminsController(
    IAdminAccountRepository adminAccounts,
    IMapper mapper) : ApiControllerBase
{
    [HttpPost("create")]
    [EndpointSummary("Create administrator account")]
    [EndpointDescription("Creates a new administrator account with a password and a comma-separated permission list. Requires the super_admin permission.")]
    [ProducesResponseType(typeof(AdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdminRequest request,
        CancellationToken cancellationToken)
    {
        if (await adminAccounts.ExistsAsync(request.Username, cancellationToken))
        {
            throw new ConflictException($"Admin account '{request.Username}' already exists.");
        }

        var account = new AdminAccount
        {
            Username = request.Username,
            PasswordHash = IdentityPasswordHasher.HashPassword(request.Password),
            Permissions = AdminPermissionNames.ParseMany(request.Permissions)
        };

        await adminAccounts.AddAsync(account, cancellationToken);

        return Created($"/api/admins/{account.Id}", mapper.Map<AdminDto>(account));
    }

    [HttpPut("{id:guid}/password")]
    [EndpointSummary("Change administrator password")]
    [EndpointDescription("Changes the password for an existing administrator account. Requires the super_admin permission.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        [FromRoute] Guid id,
        [FromBody] ChangeAdminPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var account = await adminAccounts.ChangePasswordAsync(
            id,
            IdentityPasswordHasher.HashPassword(request.Password),
            cancellationToken);
        if (account is null)
        {
            throw new NotFoundException("Admin account not found.");
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/permissions")]
    [EndpointSummary("Change administrator permissions")]
    [EndpointDescription("Replaces the administrator permission set with a comma-separated permission list. Requires the super_admin permission.")]
    [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePermissions(
        [FromRoute] Guid id,
        [FromBody] ChangeAdminPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var account = await adminAccounts.ChangePermissionsAsync(
            id,
            AdminPermissionNames.ParseMany(request.Permissions),
            cancellationToken);
        if (account is null)
        {
            throw new NotFoundException("Admin account not found.");
        }

        return Ok(mapper.Map<AdminDto>(account));
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete administrator account")]
    [EndpointDescription("Deletes an administrator account. Requires the super_admin permission.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await adminAccounts.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException("Admin account not found.");
        }

        return NoContent();
    }
}
