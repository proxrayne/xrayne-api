using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Exceptions;
using Api.Requests;
using Api.Responses;
using Contracts.Configurations;
using Contracts.Values;
using Infrastructure.Services;

namespace Api.Controllers;

[Route("api/settings")]
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
public sealed class SettingsController(
    IAppSettingsService appSettingsService,
    IPanelRestartService restartService,
    ILogger<SettingsController> logger,
    IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets application settings.
    /// </summary>
    [HttpGet("app")]
    [EndpointSummary("Get application settings")]
    [EndpointDescription("Returns subscription settings and configured notification webhooks.")]
    [ProducesResponseType(typeof(AppSettingsResponse), StatusCodes.Status200OK)]
    public async Task<AppSettingsResponse> GetAppSettings(CancellationToken ct)
    {
        var settings = await appSettingsService.GetAsync(ct);

        return mapper.Map<AppSettingsResponse>(settings);
    }

    /// <summary>
    /// Updates subscription settings.
    /// </summary>
    [HttpPut("app/subscription")]
    [EndpointSummary("Update subscription settings")]
    [EndpointDescription("Updates subscription settings and announcement without mutating webhooks.")]
    [ProducesResponseType(typeof(AppSettingsResponse), StatusCodes.Status200OK)]
    public async Task<AppSettingsResponse> UpdateSubscriptionSettings(
        [FromBody] AppSubscriptionSettingsDto request,
        CancellationToken ct)
    {
        var settings = await appSettingsService.UpdateSubscriptionAsync(
            mapper.Map<AppSettings>(request),
            ct);

        return mapper.Map<AppSettingsResponse>(settings);
    }

    /// <summary>
    /// Creates an application webhook.
    /// </summary>
    [HttpPost("app/webhooks")]
    [EndpointSummary("Create application webhook")]
    [EndpointDescription("Creates a notification webhook. Secret is accepted only during creation.")]
    [ProducesResponseType(typeof(AppWebhookDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateWebhook(
        [FromBody] CreateAppWebhookRequest request,
        CancellationToken ct)
    {
        var webhook = await appSettingsService.AddWebhookAsync(
            mapper.Map<AppWebhook>(request),
            ct);

        return Created($"/api/settings/app/webhooks/{webhook.Id}", mapper.Map<AppWebhookDto>(webhook));
    }

    /// <summary>
    /// Updates an application webhook.
    /// </summary>
    [HttpPut("app/webhooks/{id:guid}")]
    [EndpointSummary("Update application webhook")]
    [EndpointDescription("Updates a notification webhook without reading or changing its secret.")]
    [ProducesResponseType(typeof(AppWebhookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<AppWebhookDto> UpdateWebhook(
        [FromRoute] Guid id,
        [FromBody] UpdateAppWebhookRequest request,
        CancellationToken ct)
    {
        var webhook = await appSettingsService.UpdateWebhookAsync(
            id,
            mapper.Map<AppWebhook>(request),
            ct);
        if (webhook is null)
        {
            throw new NotFoundException("Webhook not found.");
        }

        return mapper.Map<AppWebhookDto>(webhook);
    }

    /// <summary>
    /// Deletes an application webhook.
    /// </summary>
    [HttpDelete("app/webhooks/{id:guid}")]
    [EndpointSummary("Delete application webhook")]
    [EndpointDescription("Deletes a notification webhook.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhook([FromRoute] Guid id, CancellationToken ct)
    {
        if (!await appSettingsService.DeleteWebhookAsync(id, ct))
        {
            throw new NotFoundException("Webhook not found.");
        }

        return NoContent();
    }

    [HttpPost("panel/restart")]
    [EndpointSummary("Restart panel process")]
    [EndpointDescription("Schedules the panel process to restart.")]
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
