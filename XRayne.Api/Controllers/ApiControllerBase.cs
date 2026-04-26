using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    protected string? Username => User.Identity?.Name;

    protected Guid? AdminId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var adminId)
                ? adminId
                : null;
        }
    }

    protected bool TryGetAdminId(out Guid adminId)
    {
        var value = AdminId;
        if (value.HasValue)
        {
            adminId = value.Value;
            return true;
        }

        adminId = Guid.Empty;
        return false;
    }
}
