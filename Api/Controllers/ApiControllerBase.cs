using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Provides shared API controller helpers.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private static readonly JsonSerializerOptions SseJsonOptions = new(JsonSerializerDefaults.Web);

    protected bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    protected string? Username => User.Identity?.Name;

    protected Guid AdminId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (value is null || !Guid.TryParse(value, out var adminId))
            {
                throw new UnauthorizedAccessException("Administrator id is missing from the access token.");
            }

            return adminId;
        }
    }

    protected Guid? AdminIdOrNull
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
        var value = AdminIdOrNull;
        if (value.HasValue)
        {
            adminId = value.Value;
            return true;
        }

        adminId = Guid.Empty;
        return false;
    }

    protected async Task WriteServerSentEventAsync<T>(T data, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(data, SseJsonOptions);

        await Response.WriteAsync($"data: {json}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }

    protected void SetupStreamHeaders()
    {
        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";
    }
}
