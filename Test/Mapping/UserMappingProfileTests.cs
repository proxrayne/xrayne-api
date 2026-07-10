using Api.Mapping;
using Api.Responses;
using AutoMapper;
using Contracts.Enums;
using Data.Entities;

namespace Test.Mapping;

/// <summary>
/// Tests user API mapping rules.
/// </summary>
public sealed class UserMappingProfileTests
{
    private readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<UserMappingProfile>()).CreateMapper();

    [Fact]
    public void Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserMappingProfile>());

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void UserEntity_MapsToUserDtos()
    {
        var user = new UserEntity
        {
            Id = 12,
            Username = "alice",
            Note = "note",
            Status = UserStatus.Active,
            DataLimit = 1024,
            ConnectionLimit = 3,
            Warehouse = new WarehouseEntity
            {
                Id = 7,
                Name = "Primary",
                Note = string.Empty,
                Enabled = true
            },
            Connections =
            [
                new ConnectionEntity { Password = "password-1" },
                new ConnectionEntity { Password = "password-2" }
            ]
        };

        var listItem = _mapper.Map<UserListItemDto>(user);
        var detail = _mapper.Map<UserDto>(user);

        listItem.ConnectionsCount.Should().Be(2);
        listItem.ConnectionLimit.Should().Be(3);
        listItem.TrafficUsedBytes.Should().Be(0);
        listItem.DataLimitBytes.Should().Be(1024);
        listItem.WarehouseId.Should().Be(7);
        listItem.WarehouseName.Should().Be("Primary");
        detail.Note.Should().Be("note");
        detail.ConnectionsCount.Should().Be(2);
    }
}
