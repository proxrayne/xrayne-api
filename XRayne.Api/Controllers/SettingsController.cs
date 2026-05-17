using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services;

namespace XRayne.Api.Controllers;

[Route("api/settings")]
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
public sealed class SettingsController(
    IMapper mapper,
    ISettingsService settingsService,
    IPanelRestartService restartService,
    ILogger<SettingsController> logger) : ApiControllerBase
{
    [HttpGet("panel")]
    [EndpointSummary("Get panel settings")]
    [ProducesResponseType(typeof(PanelSettingsResponse), StatusCodes.Status200OK)]
    public PanelSettingsResponse GetCurrent()
    {
        var settings = mapper.Map<PanelSettingsDto>(settingsService.Current);

        return new PanelSettingsResponse()
        {
            Settings = settings,
            PendingRestart = settingsService.PendingRestart,
        };
    }

    [HttpPut("panel")]
    [EndpointSummary("Update panel settings")]
    [ProducesResponseType(typeof(UpdatePanelSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<UpdatePanelSettingsResponse> UpdatePanel(
        [FromBody] UpdatePanelSettingsRequest request,
        CancellationToken ct)
    {
        var next = mapper.Map<PanelSettings>(request);
        var result = await settingsService.ApplyAsync(next, ct);

        return new UpdatePanelSettingsResponse
        {
            RequiresRestart = result.RequiresRestart,
            ChangedFields = result.ChangedFields.ToList(),
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
