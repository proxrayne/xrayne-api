using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XRayne.Api.Exceptions;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.Utilities;
using XRayne.Repositories.Contracts;

namespace XRayne.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController(
    IAdminAccountRepository adminAccounts,
    IJwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions,
    IMapper mapper) : ApiControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [EndpointSummary("Get current administrator account")]
    [EndpointDescription("Returns the administrator account associated with the current JWT access token.")]
    [ProducesResponseType(typeof(AdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (!TryGetAdminId(out var adminId))
        {
            throw new UnauthorizedException("Invalid JWT access token.");
        }

        var account = await adminAccounts.GetByIdAsync(adminId, ct);
        if (account is null)
        {
            throw new NotFoundException("Admin account not found.");
        }

        return Ok(mapper.Map<AdminDto>(account));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Authenticate administrator")]
    [EndpointDescription("Authenticates an administrator by username and password and returns a JWT access token.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var account = await adminAccounts.GetByUsernameAsync(request.Username, ct);
        if (account is null || !IdentityPasswordHasher.VerifyPassword(request.Password, account.PasswordHash))
        {
            throw new Exception("Invalid username or password.");
        }

        var updatedAccount = await adminAccounts.SetLastLoginAsync(account.Id, DateTimeOffset.UtcNow, ct);

        var lifetimeMinutes = jwtOptions.Value.AccessTokenLifetimeMinutes;
        var accessToken = jwtTokenService.CreateAccessToken(
            account.Id,
            account.Username,
            account.Permissions,
            lifetimeMinutes);
        var expireAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);

        return Ok(new LoginResponse(
            accessToken,
            expireAt,
            mapper.Map<AdminDto>(updatedAccount ?? account)));
    }
}
