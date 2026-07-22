using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Infrastructure.Services;
using OptionalValues;
using Xray.Config.Enums;

namespace Test.Infrastructure;

/// <summary>
/// Tests connection application behavior.
/// </summary>
public sealed class ConnectionServiceTests
{
    private readonly IConnectionRepository _connections = Substitute.For<IConnectionRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    [Fact]
    public async Task GetByUserIdAsync_ValidatesUserAndPassesFilterToRepository()
    {
        var user = CreateUser(connectionLimit: 2);
        var filter = new ConnectionFilter
        {
            Search = "mobile",
            IncludeRevoked = true,
            Page = 2,
            Limit = 10
        };
        var expected = new OffsetPage<ConnectionEntity>(
            [new ConnectionEntity { Id = 5, UserId = user.Id, Password = "secret" }],
            11,
            2,
            2);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _connections
            .SearchByUserIdAsync(user.Id, filter, Arg.Any<CancellationToken>())
            .Returns(expected);
        var service = CreateService();

        var page = await service.GetByUserIdAsync(user.Id, filter);

        page.Should().BeSameAs(expected);
        await _users.Received(1).GetByIdAsync(user.Id, Arg.Any<CancellationToken>());
        await _connections.Received(1).SearchByUserIdAsync(user.Id, filter, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_CreatesConnectionWithGeneratedCredentials()
    {
        var user = CreateUser(connectionLimit: 2);
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _connections
            .AddAsync(Arg.Any<ConnectionEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ConnectionEntity>()));
        var service = CreateService();

        var connection = await service.CreateAsync(
            user.Id,
            " mobile ",
            XtlsFlow.XtlsRprxVision,
            EncryptionMethod.Chacha20Poly1305,
            DeviceVerificationMethod.UserAgent);

        connection.UserId.Should().Be(user.Id);
        connection.Name.Should().Be("mobile");
        connection.Flow.Should().Be(XtlsFlow.XtlsRprxVision);
        connection.Method.Should().Be(EncryptionMethod.Chacha20Poly1305);
        connection.DeviceVerificationMethod.Should().Be(DeviceVerificationMethod.UserAgent);
        connection.Uuid.Should().NotBe(Guid.Empty);
        connection.Password.Should().NotBeNullOrWhiteSpace();
        await _connections.Received(1).AddAsync(connection, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ThrowsForbiddenException_WhenConnectionLimitIsExhausted()
    {
        var user = CreateUser(connectionLimit: 1);
        user.Connections.Add(new ConnectionEntity { UserId = user.Id, Password = "existing" });
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var service = CreateService();

        var action = () => service.CreateAsync(
            user.Id,
            "mobile",
            XtlsFlow.None,
            EncryptionMethod.None,
            DeviceVerificationMethod.None);

        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Connection limit is exhausted.");
        await _connections.DidNotReceive()
            .AddAsync(Arg.Any<ConnectionEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_IgnoresRevokedConnections_WhenCheckingConnectionLimit()
    {
        var user = CreateUser(connectionLimit: 1);
        user.Connections.Add(new ConnectionEntity
        {
            UserId = user.Id,
            Password = "existing",
            Revoked = true
        });
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _connections
            .AddAsync(Arg.Any<ConnectionEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ConnectionEntity>()));
        var service = CreateService();

        var connection = await service.CreateAsync(
            user.Id,
            "mobile",
            XtlsFlow.None,
            EncryptionMethod.None,
            DeviceVerificationMethod.None);

        connection.Revoked.Should().BeFalse();
        await _connections.Received(1).AddAsync(connection, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEditableFieldsAndPreservesCredentialsAndOwner()
    {
        var uuid = Guid.NewGuid();
        var existing = new ConnectionEntity
        {
            Id = 10,
            UserId = 42,
            Name = "old",
            Uuid = uuid,
            Password = "secret",
            Flow = XtlsFlow.None,
            Method = EncryptionMethod.None,
            DeviceVerificationMethod = DeviceVerificationMethod.None
        };
        _connections.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        _connections
            .UpdateAsync(Arg.Any<ConnectionEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ConnectionEntity>()));
        var service = CreateService();

        var connection = await service.UpdateAsync(
            existing.Id,
            "laptop",
            XtlsFlow.XtlsRprxVision,
            EncryptionMethod.Aes256Gcm,
            DeviceVerificationMethod.Combined);

        connection.UserId.Should().Be(42);
        connection.Uuid.Should().Be(uuid);
        connection.Password.Should().Be("secret");
        connection.Name.Should().Be("laptop");
        connection.Flow.Should().Be(XtlsFlow.XtlsRprxVision);
        connection.Method.Should().Be(EncryptionMethod.Aes256Gcm);
        connection.DeviceVerificationMethod.Should().Be(DeviceVerificationMethod.Combined);
    }

    [Fact]
    public async Task PatchAsync_UpdatesOnlySpecifiedFields()
    {
        var uuid = Guid.NewGuid();
        var existing = new ConnectionEntity
        {
            Id = 10,
            UserId = 42,
            Name = "old",
            Uuid = uuid,
            Password = "secret",
            Flow = XtlsFlow.None,
            Method = EncryptionMethod.Chacha20Poly1305,
            DeviceVerificationMethod = DeviceVerificationMethod.None
        };
        _connections.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        _connections
            .UpdateAsync(Arg.Any<ConnectionEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<ConnectionEntity>()));
        var service = CreateService();

        var connection = await service.PatchAsync(
            existing.Id,
            new ConnectionPatch
            {
                Name = "tablet",
                DeviceVerificationMethod = DeviceVerificationMethod.DeviceInfo
            });

        connection.UserId.Should().Be(42);
        connection.Uuid.Should().Be(uuid);
        connection.Password.Should().Be("secret");
        connection.Name.Should().Be("tablet");
        connection.Flow.Should().Be(XtlsFlow.None);
        connection.Method.Should().Be(EncryptionMethod.Chacha20Poly1305);
        connection.DeviceVerificationMethod.Should().Be(DeviceVerificationMethod.DeviceInfo);
    }

    [Fact]
    public async Task RevokeByIdAsync_RevokesConnection()
    {
        var revoked = new ConnectionEntity
        {
            Id = 7,
            UserId = 42,
            Name = "mobile",
            Password = "secret",
            Revoked = true,
            RevokedAt = DateTimeOffset.Parse("2026-07-22T10:00:00Z")
        };
        _connections
            .RevokeByIdAsync(revoked.Id, Arg.Any<CancellationToken>())
            .Returns(revoked);
        var service = CreateService();

        var connection = await service.RevokeByIdAsync(revoked.Id);

        connection.Should().BeSameAs(revoked);
        await _connections.Received(1).RevokeByIdAsync(revoked.Id, Arg.Any<CancellationToken>());
    }

    private ConnectionService CreateService()
    {
        return new ConnectionService(_connections, _users);
    }

    private static UserEntity CreateUser(uint connectionLimit)
    {
        return new UserEntity
        {
            Id = 42,
            Username = "alice",
            Status = UserStatus.Active,
            ConnectionLimit = connectionLimit
        };
    }
}
