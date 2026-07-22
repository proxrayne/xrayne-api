using Data.Entities;
using Data.Extensions;
using Xray.Config.Enums;

namespace Test.Data;

/// <summary>
/// Tests Xray client conversion helpers for connection entities.
/// </summary>
public sealed class ConnectionExtensionTests
{
    [Fact]
    public void GetUniqEmail_AppendsConnectionIdToProvidedParts()
    {
        var connection = CreateConnection(id: 42);

        var email = connection.GetUniqEmail("admin", "alice");

        email.Should().Be("admin.alice.42");
    }

    [Fact]
    public void ToVlessClient_MapsUuidFlowAndEmail()
    {
        var uuid = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var connection = CreateConnection(uuid: uuid, flow: XtlsFlow.XtlsRprxVision);

        var client = connection.ToVlessClient("client@example.com");

        client.Id.Should().Be(uuid.ToString());
        client.Flow.Should().Be(XtlsFlow.XtlsRprxVision);
        client.Email.Should().Be("client@example.com");
    }

    [Fact]
    public void ToVMessClient_MapsUuidAndEmail()
    {
        var uuid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var connection = CreateConnection(uuid: uuid);

        var client = connection.ToVMessClient("client@example.com");

        client.Id.Should().Be(uuid.ToString());
        client.Email.Should().Be("client@example.com");
    }

    [Fact]
    public void ToTrojanClient_MapsPasswordAndEmail()
    {
        var connection = CreateConnection(password: "secret");

        var client = connection.ToTrojanClient("client@example.com");

        client.Password.Should().Be("secret");
        client.Email.Should().Be("client@example.com");
    }

    [Fact]
    public void ToShadowSocksClient_MapsPasswordMethodAndEmail()
    {
        var connection = CreateConnection(
            password: "secret",
            method: EncryptionMethod.Chacha20Poly1305);

        var client = connection.ToShadowSocksClient("client@example.com");

        client.Password.Should().Be("secret");
        client.Method.Should().Be(EncryptionMethod.Chacha20Poly1305);
        client.Email.Should().Be("client@example.com");
    }

    [Fact]
    public void ToHysteriaClient_MapsPasswordAsAuthAndEmail()
    {
        var connection = CreateConnection(password: "secret");

        var client = connection.ToHysteriaClient("client@example.com");

        client.Auth.Should().Be("secret");
        client.Email.Should().Be("client@example.com");
    }

    [Fact]
    public void ConversionMethods_RequireConnection()
    {
        ConnectionEntity connection = null!;

        var act = () => connection.ToVlessClient("client@example.com");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConversionMethods_RequireEmail()
    {
        var connection = CreateConnection();

        var act = () => connection.ToVlessClient(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static ConnectionEntity CreateConnection(
        long id = 1,
        Guid? uuid = null,
        string password = "password",
        XtlsFlow flow = XtlsFlow.None,
        EncryptionMethod method = EncryptionMethod.None)
    {
        return new ConnectionEntity
        {
            Id = id,
            Uuid = uuid ?? Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Password = password,
            Flow = flow,
            Method = method
        };
    }
}
