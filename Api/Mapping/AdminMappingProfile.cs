using AutoMapper;
using Api.Responses;
using Contracts.Values;
using Octokit;
using Data.Entities;

namespace Api.Mapping;

/// <summary>
/// Maps administrator and GitHub API models.
/// </summary>
public sealed class AdminMappingProfile : Profile
{
    /// <summary>
    /// Creates administrator and GitHub mapping rules.
    /// </summary>
    public AdminMappingProfile()
    {
        MapAdmin();
        MapGithub();
    }

    private void MapAdmin()
    {
        CreateMap<AdminAccountEntity, AdminDto>()
            .ForCtorParam(
                nameof(AdminDto.Permissions),
                options => options.MapFrom(source => AdminPermissionNames.ToNames(source.Permissions)));
    }

    private void MapGithub()
    {
        CreateMap<Release, GitHubReleaseDto>()
            .ForMember(
                destination => destination.PublishedAt,
                options => options.MapFrom(source => source.PublishedAt.GetValueOrDefault()));
    }
}
