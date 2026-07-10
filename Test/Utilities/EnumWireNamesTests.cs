using System.Text.Json;
using Contracts.Enums;
using Contracts.Utilities;

namespace Test.Utilities;

public sealed class EnumWireNamesTests
{
    [Fact]
    public void GetName_ReturnsSnakeCaseWireName()
    {
        EnumWireNames.GetName(SortOrder.Asc).Should().Be("asc");
        EnumWireNames.GetName(UserSortBy.CreatedAt).Should().Be("created_at");
        EnumWireNames.GetName(UserStatus.OnHold).Should().Be("on_hold");
        EnumWireNames.GetName(SSHAuthType.PrivateKey).Should().Be("private_key");
        EnumWireNames.GetName(GeoResourceSourceType.AutoUpdate).Should().Be("auto_update");
        EnumWireNames.GetName(WebhookEvent.UserStatusChanged).Should().Be("user_status_changed");
    }

    [Fact]
    public void JsonStringEnumConverter_AcceptsNumericValues()
    {
        var value = JsonSerializer.Deserialize<UserStatus>("3");

        value.Should().Be(UserStatus.OnHold);
    }

    [Fact]
    public void JsonStringEnumConverter_RoundTripsMemberNameValues()
    {
        var json = JsonSerializer.Serialize(UserStatus.OnHold);
        var value = JsonSerializer.Deserialize<UserStatus>(json);

        json.Should().Be("\"on_hold\"");
        value.Should().Be(UserStatus.OnHold);
    }
}
