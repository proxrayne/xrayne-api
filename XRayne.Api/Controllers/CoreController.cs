using Microsoft.AspNetCore.Mvc;
using XRayne.Infrastructure.Services;

namespace XRayne.Api.Controllers;

[ApiController]
[Route("api/core")]
public sealed class CoreController(ILogger<CoreController> logger, ICoreService coreService) : ControllerBase
{
    [HttpPost("start")]
    [EndpointSummary("Start xray-core")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartXray()
    {
        logger.LogInformation("Starting xray-core.");

        await coreService.StartCore();

        return Ok("started");
    }
}
