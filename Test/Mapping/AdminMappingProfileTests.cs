using Api.Mapping;
using Api.Responses;
using AutoMapper;
using Contracts.Enums;
using Data.Entities;

namespace Test.Mapping;

/// <summary>
/// Tests administrator API mapping rules.
/// </summary>
public sealed class AdminMappingProfileTests
{
    private readonly IMapper _mapper = MapperTestFactory.CreateConfiguration(
        cfg => cfg.AddProfile<AdminMappingProfile>()).CreateMapper();

    [Fact]
    public void Configuration_IsValid()
    {
        var config = MapperTestFactory.CreateConfiguration(cfg => cfg.AddProfile<AdminMappingProfile>());

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void AdminAccountEntity_MapsToAdminDto()
    {
        var admin = new AdminAccountEntity
        {
            Id = Guid.NewGuid(),
            Username = "manager",
            Email = "manager@example.com",
            PasswordHash = "hash",
            Permissions = AdminPermission.ManageAdmins | AdminPermission.ManageWarehouses,
            CreatedAt = DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };

        var dto = _mapper.Map<AdminDto>(admin);

        dto.Id.Should().Be(admin.Id);
        dto.Username.Should().Be("manager");
        dto.Email.Should().Be("manager@example.com");
        dto.Permissions.Should().BeEquivalentTo(["manage_admins", "manage_warehouses"]);
        dto.LastLoginAt.Should().Be(admin.LastLoginAt);
    }
}
