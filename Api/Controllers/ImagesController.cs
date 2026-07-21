using Api.Responses;
using Contracts.Exceptions;
using Data.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Serves stored images as binary HTTP responses.
/// </summary>
[AllowAnonymous]
[Route("image")]
public sealed class ImagesController(
    IImageRepository images) : ApiControllerBase
{
    private const string ImmutableCacheControl = "public, max-age=31536000, immutable";
    private const string RevalidatingCacheControl = "no-cache";

    /// <summary>
    /// Gets a stored image by public key.
    /// </summary>
    [HttpGet("{key}")]
    [EndpointSummary("Get image")]
    [EndpointDescription("Get a stored image as binary content by public image key.")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(
        string key,
        [FromQuery(Name = "v")] string? version,
        CancellationToken cancellationToken)
    {
        var image = await images.GetContentByKeyOrDefaultAsync(key, cancellationToken)
            ?? throw new NotFoundException($"Image '{key}' was not found.");

        Response.Headers.CacheControl = IsCurrentVersion(version, image.Version)
            ? ImmutableCacheControl
            : RevalidatingCacheControl;

        return File(image.Content, image.ContentType);
    }

    private static bool IsCurrentVersion(string? requestedVersion, long currentVersion)
    {
        return long.TryParse(requestedVersion, out var parsedVersion)
            && parsedVersion == currentVersion;
    }
}
