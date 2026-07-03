using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Responses;

namespace Api.Controllers;

[Route("api/version")]
public sealed class VersionController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Get API version")]
    [EndpointDescription("Returns the current XRayne API version.")]
    [ProducesResponseType(typeof(ApiVersionResponse), StatusCodes.Status200OK)]
    public IActionResult GetVersion()
    {
        return Ok(new ApiVersionResponse(GetApiVersion()));
    }

    private static string GetApiVersion()
    {
        var assembly = typeof(VersionController).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion.Split('+', 2)[0];
        }

        return assembly.GetName().Version?.ToString() ?? "unknown";
    }
}
