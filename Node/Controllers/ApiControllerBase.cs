using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Node.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private static readonly JsonSerializerOptions SseJsonOptions = new(JsonSerializerDefaults.Web);

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
