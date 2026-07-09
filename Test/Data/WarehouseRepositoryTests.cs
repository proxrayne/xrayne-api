using Contracts.Enums;
using Contracts.Models;
using Data;
using Data.Entities;
using Data.Implementations;
using Microsoft.EntityFrameworkCore;
using Test.Infrastructure;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Data;

/// <summary>
/// Tests warehouse persistence behavior.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class WarehouseRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE "Admins" RESTART IDENTITY CASCADE;""");
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SearchAsync_UsesDefaultPageSizeAndReturnsCounts()
    {
        await using var context = CreateContext();
        var (admin, inbounds) = await CreateAdminWithInboundsAsync(context, 1);
        var repository = new WarehouseRepository(context);

        for (var index = 1; index <= 11; index++)
        {
            await repository.AddAsync(
                admin.Id,
                new WarehouseEntity
                {
                    Name = $"Warehouse {index:D2}",
                    Note = string.Empty,
                    Enabled = true
                },
                inbounds,
                CancellationToken.None);
        }

        var first = await repository.GetByIdAsync(admin.Id, 1, CancellationToken.None);
        context.Users.Add(new UserEntity
        {
            Username = "user-1",
            Status = UserStatus.Active,
            Admin = admin,
            Warehouse = first!
        });
        await context.SaveChangesAsync();

        var page = await repository.SearchAsync(admin.Id, new WarehouseFilter(), CancellationToken.None);

        page.Items.Should().HaveCount(10);
        page.TotalItems.Should().Be(11);
        page.CurrentPage.Should().Be(1);
        page.TotalPages.Should().Be(2);
        page.Items[0].Users.Count.Should().Be(1);
        page.Items[0].Inbounds.Count.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_FiltersByAnyInbound()
    {
        await using var context = CreateContext();
        var (admin, inbounds) = await CreateAdminWithInboundsAsync(context, 2);
        var repository = new WarehouseRepository(context);

        await repository.AddAsync(admin.Id, CreateWarehouse("First"), [inbounds[0]], CancellationToken.None);
        await repository.AddAsync(admin.Id, CreateWarehouse("Second"), [inbounds[1]], CancellationToken.None);
        await repository.AddAsync(admin.Id, CreateWarehouse("Both"), inbounds, CancellationToken.None);

        var page = await repository.SearchAsync(
            admin.Id,
            new WarehouseFilter { InboundIds = inbounds.Select(inbound => inbound.Id).ToArray() },
            CancellationToken.None);

        page.TotalItems.Should().Be(3);
        page.Items.Select(warehouse => warehouse.Name).Should().Contain(["Both", "First", "Second"]);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesAssignedInbounds()
    {
        await using var context = CreateContext();
        var (admin, inbounds) = await CreateAdminWithInboundsAsync(context, 2);
        var repository = new WarehouseRepository(context);
        var warehouse = await repository.AddAsync(
            admin.Id,
            CreateWarehouse("Original"),
            [inbounds[0]],
            CancellationToken.None);

        var updated = await repository.UpdateAsync(
            admin.Id,
            warehouse.Id,
            new WarehouseEntity
            {
                Name = "Updated",
                Note = "changed",
                Enabled = false
            },
            [inbounds[1]],
            CancellationToken.None);

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated");
        updated.Enabled.Should().BeFalse();
        updated.Inbounds.Should().ContainSingle(inbound => inbound.Id == inbounds[1].Id);
    }

    [Fact]
    public async Task HasUsersAsync_ReturnsTrueForAssignedUsers()
    {
        await using var context = CreateContext();
        var (admin, inbounds) = await CreateAdminWithInboundsAsync(context, 1);
        var repository = new WarehouseRepository(context);
        var warehouse = await repository.AddAsync(admin.Id, CreateWarehouse("Assigned"), inbounds, CancellationToken.None);

        context.Users.Add(new UserEntity
        {
            Username = "assigned",
            Status = UserStatus.Active,
            Admin = admin,
            Warehouse = warehouse
        });
        await context.SaveChangesAsync();

        var hasUsers = await repository.HasUsersAsync(admin.Id, warehouse.Id, CancellationToken.None);

        hasUsers.Should().BeTrue();
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql(fixture.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static WarehouseEntity CreateWarehouse(string name)
    {
        return new WarehouseEntity
        {
            Name = name,
            Note = string.Empty,
            Enabled = true
        };
    }

    private static async Task<(AdminAccount Admin, List<InboundEntity> Inbounds)> CreateAdminWithInboundsAsync(
        AppDbContext context,
        int inboundCount)
    {
        var admin = new AdminAccount
        {
            Username = $"admin-{Guid.NewGuid():N}",
            PasswordHash = "hash",
            Permissions = AdminPermission.SuperAdmin
        };
        var node = new NodeEntity
        {
            Name = "Node",
            Address = "node.example.com",
            Port = 22,
            ApiPort = 443,
            SSHUsername = "root",
            EncryptedApiKey = "encrypted",
            ApiKeyFingerprint = "fingerprint",
            WorkingDirectory = "/opt/xrayne",
            LastStatusChange = DateTime.UtcNow,
            Admin = admin
        };
        var inbounds = Enumerable.Range(1, inboundCount)
            .Select(index => new InboundEntity
            {
                Enabled = true,
                ReadOnly = false,
                Admin = admin,
                Node = node,
                Config = new SocksInbound
                {
                    Tag = $"inbound-{index}",
                    Listen = "0.0.0.0",
                    Port = new Port(10_000 + index),
                    Settings = new Inbound.SocksSettings
                    {
                        Auth = SocksAuth.NoAuth,
                        Udp = true
                    }
                }
            })
            .ToList();

        context.AdminAccounts.Add(admin);
        context.Nodes.Add(node);
        context.Inbounds.AddRange(inbounds);
        await context.SaveChangesAsync();

        return (admin, inbounds);
    }
}
