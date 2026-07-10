using System.Net;
using System.Net.Http.Json;
using Contracts.Enums;
using Data;
using Data.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Infrastructure;

namespace Test.Controllers;

/// <summary>
/// Tests user HTTP endpoints.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class UsersControllerTests(PostgresFixture fixture) : IAsyncLifetime
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
    public async Task Create_ReturnsForbiddenWithoutCreateUserPermission()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        using var client = await factory.CreateAuthenticatedClientAsync(AdminPermission.ViewLogs);

        var response = await client.PostAsJsonAsync("/api/users", new { username = "forbidden" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateGetUpdateAndDelete_ManageUser()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.CreateUsers | AdminPermission.EditUsers | AdminPermission.DeleteUsers);
        var warehouse = await CreateWarehouseAsync(admin);
        using var client = CreateAuthenticatedClient(factory, admin);

        var createResponse = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = "alice",
                note = "First user",
                dataLimitBytes = 1_073_741_824UL,
                limitResetStrategy = "month",
                connectionLimit = 3,
                expireAt = DateTimeOffset.UtcNow.AddDays(30),
                warehouseId = warehouse.Id
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<UserApiDto>();
        created.Should().NotBeNull();
        created!.username.Should().Be("alice");
        created.connectionsCount.Should().Be(0);
        created.connectionLimit.Should().Be(3);

        var detail = await client.GetFromJsonAsync<UserApiDto>($"/api/users/{created.id}");
        detail!.warehouseId.Should().Be(warehouse.Id);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/users/{created.id}",
            new
            {
                note = "Disabled user",
                dataLimitBytes = 0UL,
                connectionLimit = 5,
                expireAt = (DateTimeOffset?)null,
                warehouseId = warehouse.Id,
                disabled = true
            });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UserApiDto>();
        updated!.status.Should().Be("disabled");
        updated.note.Should().Be("Disabled user");
        updated.connectionLimit.Should().Be(5);
        updated.limitResetStrategy.Should().BeNull();

        var deleteResponse = await client.DeleteAsync($"/api/users/{created.id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Create_ReturnsConflictForDuplicateUsername()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.CreateUsers);
        var warehouse = await CreateWarehouseAsync(admin);
        using var client = CreateAuthenticatedClient(factory, admin);
        var payload = new
        {
            username = "duplicate",
            note = "",
            dataLimitBytes = 0UL,
            connectionLimit = 1,
            warehouseId = warehouse.Id
        };

        var first = await client.PostAsJsonAsync("/api/users", payload);
        var second = await client.PostAsJsonAsync("/api/users", payload);

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ReturnsBadRequestForMissingWarehouse()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        using var client = await factory.CreateAuthenticatedClientAsync(AdminPermission.CreateUsers);

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = "missing-warehouse",
                dataLimitBytes = 0UL,
                connectionLimit = 1,
                warehouseId = 0
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ReturnsBadRequestForNegativeOnHoldDuration()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.CreateUsers);
        var warehouse = await CreateWarehouseAsync(admin);
        using var client = CreateAuthenticatedClient(factory, admin);

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = "on-hold",
                dataLimitBytes = 0UL,
                connectionLimit = 1,
                warehouseId = warehouse.Id,
                onHoldDays = -1
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithOnHoldDaysCreatesOnHoldUserAndExtendsExpire()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.CreateUsers);
        var warehouse = await CreateWarehouseAsync(admin);
        using var client = CreateAuthenticatedClient(factory, admin);
        var expireAt = DateTimeOffset.UtcNow.AddDays(10);

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = "held-user",
                dataLimitBytes = 0UL,
                connectionLimit = 1,
                expireAt,
                warehouseId = warehouse.Id,
                onHoldDays = 5
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<UserApiDto>();
        created!.status.Should().Be("on_hold");
        created.onHoldExpire.Should().NotBeNull();
        created.expireAt.Should().BeCloseTo(expireAt.AddDays(5), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Create_AllowsTrafficLimitWithoutResetWhenExpireIsMissing()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.CreateUsers);
        var warehouse = await CreateWarehouseAsync(admin);
        using var client = CreateAuthenticatedClient(factory, admin);

        var response = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = "traffic-no-reset",
                dataLimitBytes = 1_073_741_824UL,
                connectionLimit = 1,
                warehouseId = warehouse.Id
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<UserApiDto>();
        created!.limitResetStrategy.Should().BeNull();
    }

    [Fact]
    public async Task Create_StoresResetStrategyOnlyWhenExpireIsPresent()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync(
            $"manager-{Guid.NewGuid():N}",
            AdminPermission.CreateUsers);
        var warehouse = await CreateWarehouseAsync(admin);
        using var client = CreateAuthenticatedClient(factory, admin);

        var withoutExpire = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = "reset-without-expire",
                dataLimitBytes = 0UL,
                limitResetStrategy = "month",
                connectionLimit = 1,
                warehouseId = warehouse.Id
            });
        var withExpire = await client.PostAsJsonAsync(
            "/api/users",
            new
            {
                username = "reset-with-expire",
                dataLimitBytes = 0UL,
                limitResetStrategy = "week",
                connectionLimit = 1,
                expireAt = DateTimeOffset.UtcNow.AddDays(1),
                warehouseId = warehouse.Id
            });

        withoutExpire.StatusCode.Should().Be(HttpStatusCode.Created);
        withExpire.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdWithoutExpire = await withoutExpire.Content.ReadFromJsonAsync<UserApiDto>();
        var createdWithExpire = await withExpire.Content.ReadFromJsonAsync<UserApiDto>();
        createdWithoutExpire!.limitResetStrategy.Should().BeNull();
        createdWithExpire!.limitResetStrategy.Should().Be("week");
    }

    [Fact]
    public async Task GetAll_FiltersByStatusAndSearch()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync($"manager-{Guid.NewGuid():N}");
        var warehouse = await CreateWarehouseAsync(admin);
        await CreateUserAsync(admin, warehouse, "target-active", UserStatus.Active);
        await CreateUserAsync(admin, warehouse, "target-disabled", UserStatus.Disabled);
        await CreateUserAsync(admin, warehouse, "other-active", UserStatus.Active);
        using var client = CreateAuthenticatedClient(factory, admin);

        var response = await client.GetFromJsonAsync<PageApiDto<UserListItemApiDto>>(
            "/api/users?search=target&statuses=active");

        response!.totalItems.Should().Be(1);
        response.items.Should().ContainSingle(item => item.username == "target-active");
    }

    [Fact]
    public async Task GetAll_AcceptsSnakeCaseSortAndStatusQueryEnums()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        var admin = await factory.CreateAdminAsync($"manager-{Guid.NewGuid():N}");
        var warehouse = await CreateWarehouseAsync(admin);
        await CreateUserAsync(admin, warehouse, "held-user", UserStatus.OnHold);
        using var client = CreateAuthenticatedClient(factory, admin);

        var response = await client.GetFromJsonAsync<PageApiDto<UserListItemApiDto>>(
            "/api/users?statuses=on_hold&sortBy=created_at&sortOrder=asc");

        response!.totalItems.Should().Be(1);
        response.items.Should().ContainSingle(item => item.status == "on_hold");
    }

    [Fact]
    public async Task GetAll_RejectsCamelCaseQueryEnums()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        using var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/users?statuses=onHold&sortBy=createdAt");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_AcceptsNumericQueryEnums()
    {
        using var factory = new XRayneWebApplicationFactory(fixture.ConnectionString);
        using var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/users?sortOrder=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql(fixture.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static HttpClient CreateAuthenticatedClient(
        XRayneWebApplicationFactory factory,
        AdminAccount admin)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new(
            "Bearer",
            factory.Services.GetRequiredService<IJwtTokenService>()
                .CreateAccessToken(admin.Id, admin.Username, admin.Permissions));

        return client;
    }

    private async Task<WarehouseEntity> CreateWarehouseAsync(AdminAccount admin)
    {
        await using var context = CreateContext();
        context.Attach(admin);
        var warehouse = new WarehouseEntity
        {
            Name = $"Warehouse {Guid.NewGuid():N}",
            Note = string.Empty,
            Enabled = true,
            Admin = admin
        };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        return warehouse;
    }

    private async Task CreateUserAsync(
        AdminAccount admin,
        WarehouseEntity warehouse,
        string username,
        UserStatus status)
    {
        await using var context = CreateContext();
        context.Attach(admin);
        context.Attach(warehouse);
        context.Users.Add(new UserEntity
        {
            Username = username,
            Status = status,
            ConnectionLimit = 1,
            Admin = admin,
            Warehouse = warehouse
        });
        await context.SaveChangesAsync();
    }

    private sealed record PageApiDto<T>(
        List<T> items,
        int totalItems,
        int currentPage,
        int totalPages);

    private sealed record UserListItemApiDto(
        long id,
        string username,
        string status,
        int connectionsCount,
        uint connectionLimit,
        ulong trafficUsedBytes,
        ulong dataLimitBytes,
        long warehouseId,
        string warehouseName);

    private sealed record UserApiDto(
        long id,
        string username,
        string note,
        string status,
        int connectionsCount,
        uint connectionLimit,
        ulong trafficUsedBytes,
        ulong dataLimitBytes,
        string? limitResetStrategy,
        DateTimeOffset? expireAt,
        DateTimeOffset? onHoldExpire,
        long warehouseId,
        string warehouseName);
}
