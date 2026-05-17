using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services.PanelSettings;

namespace XRayne.Api.Controllers;

[Route("api/settings")]
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
public sealed class SettingsController(
    IPanelSettingsAccessor accessor,
    IMapper mapper,
    IPanelRestartService restartService,
    ILogger<SettingsController> logger) : ApiControllerBase
{
    [HttpGet("panel")]
    [EndpointSummary("Get panel settings")]
    [ProducesResponseType(typeof(PanelSettingsResponse), StatusCodes.Status200OK)]
    public PanelSettingsResponse GetPanel()
    {
        var response = mapper.Map<PanelSettingsResponse>(accessor.Current);
        response.PendingRestart = accessor.PendingRestart;
        return response;
    }

    [HttpPut("panel")]
    [EndpointSummary("Update panel settings")]
    [ProducesResponseType(typeof(UpdatePanelSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<UpdatePanelSettingsResponse> UpdatePanel(
        [FromBody] UpdatePanelSettingsRequest request,
        CancellationToken ct)
    {
        var next = mapper.Map<PanelOptions>(request);
        var result = await accessor.ApplyAsync(next, ct);

        return new UpdatePanelSettingsResponse
        {
            RequiresRestart = result.RequiresRestart,
            ChangedFields = result.ChangedFields.ToList(),
            HotReloaded = result.HotReloadedFields.ToList()
        };
    }

    [HttpPost("panel/restart")]
    [EndpointSummary("Restart panel process")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult RestartPanel()
    {
        var initiator = Username ?? "<unknown>";
        if (restartService.ScheduleRestart())
        {
            logger.LogInformation("Panel restart requested by {Username}.", initiator);
        }
        else
        {
            logger.LogInformation("Panel restart already pending; request by {Username} ignored.", initiator);
        }

        return Accepted();
    }
}
