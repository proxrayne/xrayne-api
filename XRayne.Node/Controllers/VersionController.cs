using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using XRayne.Node.Models;

namespace XRayne.Node.Controllers;

[ApiController]
[Route("api/version")]
public sealed class VersionController(IHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Node version")]
    [EndpointDescription("Get XRayne Node service version information.")]
    [ProducesResponseType(typeof(VersionResponse), StatusCodes.Status200OK)]
    public VersionResponse GetVersion()
    {
        var assembly = typeof(Program).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";

        return new VersionResponse("XRayne.Node", version, environment.EnvironmentName);
    }
}
