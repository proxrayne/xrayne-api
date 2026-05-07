using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services;

namespace XRayne.Api.Controllers;

[Route("api/core")]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
public sealed class CoreController(ILogger<CoreController> logger, ICoreService coreService) : ApiControllerBase
{
    [HttpPost("start")]
    [EndpointSummary("Start xray-core")]
    [EndpointDescription("Starts the xray-core instance managed by this node. Requires the change_xray_settings permission or super_admin.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartXray()
    {
        logger.LogInformation("Starting xray-core.");

        await coreService.StartCore();

        return Ok("started");
    }
}
