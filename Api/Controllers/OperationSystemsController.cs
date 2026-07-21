using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Exceptions;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Data.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Manages operating system options.
/// </summary>
[Authorize]
[Route("api/operation-systems")]
public sealed class OperationSystemsController(
    IOperationSystemRepository operationSystems,
    IMapper mapper) : ApiControllerBase
{
    /// <summary>
    /// Gets all operating systems.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List operating systems")]
    [EndpointDescription("Get all operating system options with images.")]
    [ProducesResponseType(typeof(List<OperationSystemDto>), StatusCodes.Status200OK)]
    public async Task<List<OperationSystemDto>> GetAll(CancellationToken cancellationToken)
    {
        var items = await operationSystems.GetAllAsync(cancellationToken);

        return mapper.Map<List<OperationSystemDto>>(items);
    }

    /// <summary>
    /// Gets one operating system by id.
    /// </summary>
    [HttpGet("{id}")]
    [EndpointSummary("Get operating system")]
    [EndpointDescription("Get one operating system option for editing.")]
    [ProducesResponseType(typeof(OperationSystemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<OperationSystemDto> GetById(string id, CancellationToken cancellationToken)
    {
        var operationSystem = await operationSystems.GetByIdAsync(NormalizeId(id), cancellationToken);

        return mapper.Map<OperationSystemDto>(operationSystem);
    }

    /// <summary>
    /// Creates an operating system.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Create operating system")]
    [EndpointDescription("Create an operating system option.")]
    [ProducesResponseType(typeof(OperationSystemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromForm] CreateOperationSystemFormRequest request,
        CancellationToken cancellationToken)
    {
        var id = NormalizeId(request.Id);

        ValidateRequiredText(request.Name, "Operating system name is required.");
        var imagePayload = await ImagePayloads.NormalizeForWriteAsync(
            request.ImageFile,
            required: true,
            cancellationToken)
            ?? throw new InvalidOperationException("Required image payload was not created.");

        if (await operationSystems.GetByIdOrDefaultAsync(id, cancellationToken) is not null)
        {
            throw new ConflictException($"Operating system '{id}' already exists.");
        }

        var operationSystem = new OperationSystemEntity
        {
            Id = id,
            Name = request.Name.Trim(),
            Note = request.Note?.Trim() ?? string.Empty,
            Enabled = request.Enabled,
            Image = new ImageEntity
            {
                Key = id,
                Alt = request.ImageAlt?.Trim(),
                Content = imagePayload.Content,
                ContentType = imagePayload.ContentType,
                Version = 1,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        var created = await operationSystems.AddAsync(operationSystem, cancellationToken);

        return Created($"/api/operation-systems/{created.Id}", mapper.Map<OperationSystemDto>(created));
    }

    /// <summary>
    /// Updates an operating system.
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Update operating system")]
    [EndpointDescription("Update an operating system option.")]
    [ProducesResponseType(typeof(OperationSystemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<OperationSystemDto> Update(
        string id,
        [FromForm] UpdateOperationSystemFormRequest request,
        CancellationToken ct)
    {
        var normalizedId = NormalizeId(id);

        ValidateRequiredText(request.Name, "Operating system name is required.");

        var imagePayload = await ImagePayloads.NormalizeForWriteAsync(
            request.ImageFile,
            required: false,
            ct);

        var updated = await operationSystems.UpdateAsync(
            normalizedId,
            new OperationSystemEntity
            {
                Id = normalizedId,
                Name = request.Name.Trim(),
                Note = request.Note.Trim(),
                Enabled = request.Enabled,
                Image = new ImageEntity
                {
                    Alt = request.ImageAlt?.Trim(),
                    Content = imagePayload?.Content ?? [],
                    ContentType = imagePayload?.ContentType ?? string.Empty,
                    Key = normalizedId
                }
            },
            ct);

        return mapper.Map<OperationSystemDto>(updated);
    }

    /// <summary>
    /// Partially updates an operating system.
    /// </summary>
    [HttpPatch("{id}")]
    [EndpointSummary("Patch operating system")]
    [EndpointDescription("Partially update an operating system option using only fields present in the request body.")]
    [ProducesResponseType(typeof(OperationSystemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<OperationSystemDto> Patch(
        string id,
        [FromBody] PatchOperationSystemRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedId = NormalizeId(id);

        if (request.Name.IsSpecified)
        {
            ValidateRequiredText(request.Name.SpecifiedValue, "Operating system name is required.");
        }

        var updated = await operationSystems.UpdateAsync(
            normalizedId,
            mapper.Map<OperationSystemPatch>(request),
            cancellationToken);

        return mapper.Map<OperationSystemDto>(updated);
    }

    private static string NormalizeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BadRequestException("Operating system id is required.");
        }

        return id.Trim().ToLowerInvariant();
    }

    private static void ValidateRequiredText(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException(message);
        }
    }
}
