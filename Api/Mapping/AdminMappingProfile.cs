using AutoMapper;
using Api.Responses;
using Contracts.Values;
using Octokit;
using Repositories.Entities;

namespace Api.Mapping;

public sealed class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        MapAdmin();
        MapGithub();
    }

    private void MapAdmin()
    {
        CreateMap<AdminAccount, AdminDto>()
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
