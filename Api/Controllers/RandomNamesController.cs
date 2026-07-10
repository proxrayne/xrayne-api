using Api.Responses;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Generates random readable names.
/// </summary>
[Authorize]
[Route("api/random-names")]
public sealed class RandomNamesController : ApiControllerBase
{
    /// <summary>
    /// Generates a new random name.
    /// </summary>
    [HttpGet("generate")]
    [EndpointSummary("Generate random name")]
    [EndpointDescription("Generates a random human-readable name for assigning to new resources.")]
    [ProducesResponseType(typeof(RandomNameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public RandomNameResponse Generate()
    {
        return new RandomNameResponse(RandomNameGenerator.Generate());
    }
}
