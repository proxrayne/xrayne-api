using AutoMapper;
using Github;
using XRayne.Api.Responses;
using XRayne.Contracts.Values;
using XRayne.Repositories.Entities;

namespace XRayne.Api.Mapping;

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
        CreateMap<GitHubRelease, GitHubReleaseDto>();
    }
}
