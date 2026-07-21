using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Exceptions;
using Contracts.Utilities;
using Contracts.Values;
using Data.Contracts;
using Infrastructure.Services;
using Infrastructure.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xray.Config.Share.v2Ray;

namespace Api.Controllers;

/// <summary>
/// Provides Xray configuration helper endpoints that do not control a local xray-core runtime.
/// </summary>
[Route("api/xray")]
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
public sealed class XrayController(
    IMapper mapper,
    INodeRepository nodeRepository) : ApiControllerBase
{
    private readonly GitHubReleaseClient xrayRepository = new(CoreDefaults.XrayRepositoryUrl);

    /// <summary>
    /// Gets available xray-core releases for remote node installation.
    /// </summary>
    [HttpGet("{id:long}/core/releases")]
    [EndpointSummary("Remote node Xray releases")]
    [EndpointDescription("Get available xray-core releases for remote node installation.")]
    [ProducesResponseType(typeof(List<GitHubReleaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<List<GitHubReleaseDto>> GetCoreReleases(
        long id,
        [FromQuery] CoreReleasesQuery query,
        CancellationToken ct)
    {
        if (!await nodeRepository.ExistByIdAsync(id, ct))
        {
            throw new NotFoundException($"Node '{id}' was not found.");
        }

        var releases = await xrayRepository.GetReleasesAsync(query.PerPage, query.Page, ct);

        return releases.Select(mapper.Map<GitHubReleaseDto>).ToList();
    }

    /// <summary>
    /// Decodes a v2ray share link to an outbound JSON configuration.
    /// </summary>
    [HttpPost("v2ray/decode")]
    [EndpointSummary("Decode v2ray link")]
    [EndpointDescription("Decode v2ray link to JSON outbound configuration.")]
    [ProducesResponseType(typeof(DecodeV2RayLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public DecodeV2RayLinkResponse DecodeV2RayLink([FromBody] DecodeV2RayLinkRequest request)
    {
        var share = V2RayShareEntity.FromString(request.Link);
        var outbound = share.ToOutbound(remark: request.Remark, email: request.Email);
        if (outbound is null)
        {
            throw new BadRequestException("Invalid operation result.");
        }

        return new DecodeV2RayLinkResponse(XrayJsonSerializer.Serialize(outbound));
    }
}
