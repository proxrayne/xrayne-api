using System.Security.Claims;
using Data.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Auth;

/// <summary>
/// Validates administrator JWT tokens against the current account state.
/// </summary>
public static class AdminJwtValidation
{
    /// <summary>
    /// Rejects tokens whose administrator account no longer exists or has been soft deleted.
    /// </summary>
    public static async Task ValidateActiveAdminAsync(TokenValidatedContext context)
    {
        var value = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (value is null || !Guid.TryParse(value, out var adminId))
        {
            context.Fail("Administrator id is missing from the access token.");
            return;
        }

        var adminAccounts = context.HttpContext.RequestServices.GetRequiredService<IAdminAccountRepository>();
        if (await adminAccounts.GetActiveByIdAsync(adminId, context.HttpContext.RequestAborted) is null)
        {
            context.Fail("Administrator account is inactive or deleted.");
        }
    }
}
