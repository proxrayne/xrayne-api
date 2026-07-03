using Microsoft.Extensions.Primitives;
using Node.Values;

namespace Node.Security;

public sealed class ApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipAuthentication(context.Request.Path))
        {
            await next(context);
            return;
        }

        var configuredApiKey = configuration["Node:ApiKey"];
        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(configuredApiKey))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyAuthentication.HeaderName, out StringValues providedApiKey)
            || !string.Equals(providedApiKey.ToString(), configuredApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ApiErrorResponse("unauthorized", "A valid node API key is required."));
            return;
        }

        await next(context);
    }

    private static bool ShouldSkipAuthentication(PathString path)
    {
        return path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ApiErrorResponse(string Error, string Detail);
}
