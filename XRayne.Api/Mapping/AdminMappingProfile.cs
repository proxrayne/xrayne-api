using AutoMapper;
using XRayne.Api.Responses;
using XRayne.Core.Auth;
using XRayne.Repositories.Entities;

namespace XRayne.Api.Mapping;

public sealed class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        CreateMap<AdminAccount, AdminDto>()
            .ForCtorParam(
                nameof(AdminDto.Permissions),
                options => options.MapFrom(source => AdminPermissionNames.ToNames(source.Permissions)));
    }
}
