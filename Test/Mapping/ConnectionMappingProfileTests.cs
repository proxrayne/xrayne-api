using Api.Mapping;
using Api.Responses;
using AutoMapper;
using Contracts.Enums;
using Data.Entities;
using Xray.Config.Enums;

namespace Test.Mapping;

/// <summary>
/// Tests user connection API mapping rules.
/// </summary>
public sealed class ConnectionMappingProfileTests
{
    private readonly IMapper _mapper = MapperTestFactory.CreateConfiguration(
        cfg => cfg.AddProfile<ConnectionMappingProfile>()).CreateMapper();

    [Fact]
    public void Configuration_IsValid()
    {
        var config = MapperTestFactory.CreateConfiguration(cfg => cfg.AddProfile<ConnectionMappingProfile>());

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void ConnectionEntity_MapsToConnectionDto()
    {
        var connection = new ConnectionEntity
        {
            Id = 12,
            UserId = 42,
            Name = "mobile",
            Password = "secret",
            Uuid = Guid.NewGuid(),
            Flow = XtlsFlow.XtlsRprxVision,
            Method = EncryptionMethod.Chacha20Poly1305,
            DeviceVerificationMethod = DeviceVerificationMethod.DeviceInfo,
            Revoked = true,
            CreatedAt = DateTimeOffset.Parse("2026-07-22T10:00:00Z")
        };

        var dto = _mapper.Map<ConnectionDto>(connection);

        dto.Id.Should().Be(12);
        dto.UserId.Should().Be(42);
        dto.Name.Should().Be("mobile");
        dto.Flow.Should().Be(XtlsFlow.XtlsRprxVision);
        dto.Method.Should().Be(EncryptionMethod.Chacha20Poly1305);
        dto.DeviceVerificationMethod.Should().Be(DeviceVerificationMethod.DeviceInfo);
        dto.Revoked.Should().BeTrue();
    }
}
