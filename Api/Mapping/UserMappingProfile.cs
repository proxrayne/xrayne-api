using Api.Responses;
using AutoMapper;
using Data.Entities;

namespace Api.Mapping;

/// <summary>
/// Maps subscription user API models.
/// </summary>
public sealed class UserMappingProfile : Profile
{
    /// <summary>
    /// Creates subscription user API mapping rules.
    /// </summary>
    public UserMappingProfile()
    {
        CreateMap<UserEntity, UserListItemDto>()
            .ForCtorParam(
                nameof(UserListItemDto.ConnectionsCount),
                options => options.MapFrom(user => user.Connections.Count))
            .ForCtorParam(
                nameof(UserListItemDto.TrafficUsedBytes),
                options => options.MapFrom(_ => 0UL))
            .ForCtorParam(
                nameof(UserListItemDto.DataLimitBytes),
                options => options.MapFrom(user => user.DataLimit))
            .ForCtorParam(
                nameof(UserListItemDto.WarehouseId),
                options => options.MapFrom(user => user.WarehouseId))
            .ForCtorParam(
                nameof(UserListItemDto.WarehouseName),
                options => options.MapFrom(user => user.Warehouse.Name));

        CreateMap<UserEntity, UserDto>()
            .ForCtorParam(
                nameof(UserDto.ConnectionsCount),
                options => options.MapFrom(user => user.Connections.Count))
            .ForCtorParam(
                nameof(UserDto.TrafficUsedBytes),
                options => options.MapFrom(_ => 0UL))
            .ForCtorParam(
                nameof(UserDto.DataLimitBytes),
                options => options.MapFrom(user => user.DataLimit))
            .ForCtorParam(
                nameof(UserDto.WarehouseId),
                options => options.MapFrom(user => user.WarehouseId))
            .ForCtorParam(
                nameof(UserDto.WarehouseName),
                options => options.MapFrom(user => user.Warehouse.Name));

        CreateMap<WarehouseEntity, UserWarehouseOptionDto>();
    }
}
