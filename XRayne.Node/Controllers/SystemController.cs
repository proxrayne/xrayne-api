using Microsoft.AspNetCore.Mvc;
using XRayne.Node.Models;

namespace XRayne.Node.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("status")]
    [EndpointSummary("System status")]
    [EndpointDescription("Get system status placeholder.")]
    [ProducesResponseType(typeof(SystemStatusResponse), StatusCodes.Status200OK)]
    public SystemStatusResponse GetStatus()
    {
        return new SystemStatusResponse("not_implemented", DateTimeOffset.UtcNow);
    }
}
