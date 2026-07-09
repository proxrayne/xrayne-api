using System.Net;
using System.Net.Http.Json;
using Contracts.Enums;
using Data;
using Data.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Infrastructure;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Controllers;

/// <summary>
/// Tests warehouse HTTP endpoints.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class WarehousesControllerTests(PostgresFixture fixture) : IAsyncLifetime
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
    public async Task GetAll_ReturnsForbiddenWithoutWarehousePermission()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        using var client = await factory.CreateAuthenticatedClientAsync(AdminPermission.ViewLogs);

        var response = await client.GetAsync("/api/warehouses");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAndGetById_ReturnWarehouse()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.ManageWarehouses);
        var inbound = await CreateInboundAsync(admin);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new(
            "Bearer",
            factory.Services.GetRequiredService<IJwtTokenService>()
                .CreateAccessToken(admin.Id, admin.Username, admin.Permissions));

        var createResponse = await client.PostAsJsonAsync(
            "/api/warehouses",
            new
            {
                name = "Primary",
                note = "Main group",
                enabled = true,
                inboundIds = new[] { inbound.Id }
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<WarehouseApiDto>();
        created.Should().NotBeNull();
        created!.inbounds.Should().ContainSingle(item => item.id == inbound.Id);

        var detail = await client.GetFromJsonAsync<WarehouseApiDto>($"/api/warehouses/{created.id}");

        detail!.name.Should().Be("Primary");
        detail.inbounds.Should().ContainSingle(item => item.nodeName == "Node");
    }

    [Fact]
    public async Task Delete_ReturnsConflictWhenUsersAreAssigned()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.ManageWarehouses);
        var warehouse = await CreateWarehouseWithUserAsync(admin);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new(
            "Bearer",
            factory.Services.GetRequiredService<IJwtTokenService>()
                .CreateAccessToken(admin.Id, admin.Username, admin.Permissions));

        var response = await client.DeleteAsync($"/api/warehouses/{warehouse.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetInboundsByNode_GroupsOptions()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.ManageWarehouses);
        var inbound = await CreateInboundAsync(admin);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new(
            "Bearer",
            factory.Services.GetRequiredService<IJwtTokenService>()
                .CreateAccessToken(admin.Id, admin.Username, admin.Permissions));

        var groups = await client.GetFromJsonAsync<List<WarehouseNodeInboundsApiDto>>(
            "/api/warehouses/inbounds-by-node");

        groups.Should().ContainSingle(group => group.nodeName == "Node");
        groups![0].inbounds.Should().ContainSingle(item => item.id == inbound.Id);
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql(fixture.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private async Task<InboundEntity> CreateInboundAsync(AdminAccount admin)
    {
        await using var context = CreateContext();
        context.Attach(admin);
        var node = CreateNode(admin);
        var inbound = CreateInbound(admin, node, "inbound-1");
        context.Nodes.Add(node);
        context.Inbounds.Add(inbound);
        await context.SaveChangesAsync();

        return inbound;
    }

    private async Task<WarehouseEntity> CreateWarehouseWithUserAsync(AdminAccount admin)
    {
        await using var context = CreateContext();
        context.Attach(admin);
        var warehouse = new WarehouseEntity
        {
            Name = "Assigned",
            Note = string.Empty,
            Enabled = true,
            Admin = admin
        };
        var user = new UserEntity
        {
            Username = "assigned",
            Status = UserStatus.Active,
            Admin = admin,
            Warehouse = warehouse
        };
        context.Warehouses.Add(warehouse);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return warehouse;
    }

    private static NodeEntity CreateNode(AdminAccount admin)
    {
        return new NodeEntity
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
    }

    private static InboundEntity CreateInbound(AdminAccount admin, NodeEntity node, string tag)
    {
        return new InboundEntity
        {
            Enabled = true,
            ReadOnly = false,
            Admin = admin,
            Node = node,
            Config = new SocksInbound
            {
                Tag = tag,
                Listen = "0.0.0.0",
                Port = new Port(10_001),
                Settings = new Inbound.SocksSettings
                {
                    Auth = SocksAuth.NoAuth,
                    Udp = true
                }
            }
        };
    }

    private sealed record WarehouseApiDto(
        long id,
        string name,
        string note,
        bool enabled,
        List<WarehouseInboundApiDto> inbounds,
        int usersCount);

    private sealed record WarehouseInboundApiDto(
        int id,
        string tag,
        string port,
        string protocol,
        string? network,
        string? security,
        bool enabled,
        long nodeId,
        string nodeName);

    private sealed record WarehouseNodeInboundsApiDto(
        long nodeId,
        string nodeName,
        List<WarehouseInboundApiDto> inbounds);
}
