using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Exceptions;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Data.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Manages client application profiles.
/// </summary>
[Authorize]
[Route("api/applications")]
public sealed class ApplicationsController(
    IApplicationRepository applications,
    IOperationSystemRepository operationSystems,
    IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets all applications.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List applications")]
    [EndpointDescription("Get all application profiles with images and supported operating systems.")]
    [ProducesResponseType(typeof(List<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<List<ApplicationDto>> GetAll(CancellationToken cancellationToken)
    {
        var items = await applications.GetAllAsync(cancellationToken);

        return mapper.Map<List<ApplicationDto>>(items);
    }

    /// <summary>
    /// Gets one application by id.
    /// </summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("Get application")]
    [EndpointDescription("Get one application profile for editing.")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ApplicationDto> GetById(int id, CancellationToken cancellationToken)
    {
        var application = await applications.GetByIdAsync(id, cancellationToken);

        return mapper.Map<ApplicationDto>(application);
    }

    /// <summary>
    /// Creates an application.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Create application")]
    [EndpointDescription("Create an application profile and link supported operating systems.")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromForm] CreateApplicationFormRequest request,
        CancellationToken ct)
    {
        ValidateRequiredText(request.Name, "Application name is required.");
        ValidateRequiredText(request.DetectPattern, "Detect pattern is required.");
        var imagePayload = await ImagePayloads.NormalizeForWriteAsync(
            request.ImageFile,
            required: true,
            ct)
            ?? throw new InvalidOperationException("Required image payload was not created.");

        var operationSystems = await ResolveOperationSystemsAsync(request.OperationSystemIds, ct);
        var application = new ApplicationEntity
        {
            Name = request.Name.Trim(),
            WebsiteUrl = request.WebsiteUrl?.Trim(),
            Protocol = request.Protocol?.Trim(),
            DetectPattern = request.DetectPattern.Trim(),
            SubscriptionFormat = request.SubscriptionFormat,
            Enabled = request.Enabled,
            OperationSystems = operationSystems,
            Assets = NormalizeList(request.Assets),
            Image = new ImageEntity
            {
                Key = ImageKeys.New(),
                Alt = request.ImageAlt?.Trim(),
                Content = imagePayload.Content,
                ContentType = imagePayload.ContentType,
                Version = 1,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        var created = await applications.AddAsync(application, ct);

        return Created($"/api/applications/{created.Id}", mapper.Map<ApplicationDto>(created));
    }

    /// <summary>
    /// Updates an application.
    /// </summary>
    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Update application")]
    [EndpointDescription("Update an application profile and replace its linked operating systems.")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ApplicationDto> Update(
        int id,
        [FromForm] UpdateApplicationFormRequest request,
        CancellationToken ct)
    {
        ValidateRequiredText(request.Name, "Application name is required.");
        ValidateRequiredText(request.DetectPattern, "Detect pattern is required.");
        var imagePayload = await ImagePayloads.NormalizeForWriteAsync(
            request.ImageFile,
            required: false,
            ct);

        var linkedOperationSystems = await ResolveOperationSystemsAsync(
            request.OperationSystemIds,
            ct);

        var updated = await applications.UpdateAsync(
            id,
            new ApplicationEntity
            {
                Name = request.Name.Trim(),
                WebsiteUrl = request.WebsiteUrl?.Trim(),
                Protocol = request.Protocol?.Trim(),
                DetectPattern = request.DetectPattern.Trim(),
                SubscriptionFormat = request.SubscriptionFormat,
                Enabled = request.Enabled,
                Assets = NormalizeList(request.Assets),
                OperationSystems = linkedOperationSystems,
                Image = new ImageEntity
                {
                    Alt = request.ImageAlt?.Trim(),
                    Content = imagePayload?.Content ?? [],
                    ContentType = imagePayload?.ContentType ?? string.Empty,
                    Key = string.Empty
                }
            },
            ct);

        return mapper.Map<ApplicationDto>(updated);
    }

    /// <summary>
    /// Partially updates an application.
    /// </summary>
    [HttpPatch("{id:int}")]
    [EndpointSummary("Patch application")]
    [EndpointDescription("Partially update an application profile using only fields present in the request body.")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ApplicationDto> Patch(
        int id,
        [FromBody] PatchApplicationRequest request,
        CancellationToken ct)
    {
        if (request.Name.IsSpecified)
        {
            ValidateRequiredText(request.Name.SpecifiedValue, "Application name is required.");
        }

        if (request.DetectPattern.IsSpecified)
        {
            ValidateRequiredText(request.DetectPattern.SpecifiedValue, "Detect pattern is required.");
        }

        if (request.Assets.IsSpecified && request.Assets.SpecifiedValue is null)
        {
            throw new BadRequestException("Application assets are required.");
        }

        if (request.OperationSystemIds.IsSpecified && request.OperationSystemIds.SpecifiedValue is null)
        {
            throw new BadRequestException("Operating system identifiers are required.");
        }

        var updated = await applications.UpdateAsync(id, mapper.Map<ApplicationPatch>(request), ct);

        return mapper.Map<ApplicationDto>(updated);
    }

    /// <summary>
    /// Deletes an application.
    /// </summary>
    [HttpDelete("{id:int}")]
    [EndpointSummary("Delete application")]
    [EndpointDescription("Delete an application profile.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await applications.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    private async Task<List<OperationSystemEntity>> ResolveOperationSystemsAsync(
        IReadOnlyCollection<string> operationSystemIds,
        CancellationToken ct)
    {
        var ids = operationSystemIds
            .Select(id => id.Trim())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (ids.Length == 0)
        {
            throw new BadRequestException("At least one operating system is required.");
        }

        var resolved = await operationSystems.GetByIdsAsync(ids, ct);
        if (resolved.Count != ids.Length)
        {
            throw new BadRequestException("One or more selected operating systems were not found.");
        }

        return resolved;
    }

    private static void ValidateRequiredText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException(message);
        }
    }

    private static List<string> NormalizeList(IEnumerable<string> values)
    {
        return values
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
