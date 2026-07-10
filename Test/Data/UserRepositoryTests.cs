using Contracts.Enums;
using Contracts.Models;
using Data;
using Data.Entities;
using Data.Implementations;
using Microsoft.EntityFrameworkCore;
using Test.Infrastructure;

namespace Test.Data;

/// <summary>
/// Tests user persistence behavior.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class UserRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
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
    public void UserFilter_DoesNotExposeProtocol()
    {
        typeof(UserFilter).GetProperty("Protocol").Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_FiltersByUsernameStatusAndPaginates()
    {
        await using var context = CreateContext();
        var (admin, warehouse) = await CreateAdminWithWarehouseAsync(context);
        var repository = new UserRepository(context);

        for (var index = 1; index <= 5; index++)
        {
            context.Users.Add(CreateUser(admin, warehouse, $"match-active-{index}", UserStatus.Active));
        }

        for (var index = 1; index <= 4; index++)
        {
            context.Users.Add(CreateUser(admin, warehouse, $"match-disabled-{index}", UserStatus.Disabled));
        }

        context.Users.Add(CreateUser(admin, warehouse, "other-active", UserStatus.Active));
        await context.SaveChangesAsync();

        var page = await repository.SearchAsync(
            admin.Id,
            new UserFilter
            {
                Search = "match",
                Status = [UserStatus.Active],
                Page = 2,
                Limit = 2
            },
            CancellationToken.None);

        page.Items.Should().HaveCount(2);
        page.TotalItems.Should().Be(5);
        page.CurrentPage.Should().Be(2);
        page.TotalPages.Should().Be(3);
        page.Items.Should().OnlyContain(user => user.Username.Contains("match-active"));
    }

    [Theory]
    [InlineData(UserSortBy.Username, SortOrder.Asc, "alpha")]
    [InlineData(UserSortBy.Username, SortOrder.Desc, "gamma")]
    [InlineData(UserSortBy.Status, SortOrder.Desc, "disabled")]
    [InlineData(UserSortBy.Traffic, SortOrder.Desc, "gamma")]
    [InlineData(UserSortBy.Connections, SortOrder.Desc, "alpha")]
    public async Task SearchAsync_SortsByRequestedColumn(
        UserSortBy sortBy,
        SortOrder sortOrder,
        string expectedFirstUsername)
    {
        await using var context = CreateContext();
        var (admin, warehouse) = await CreateAdminWithWarehouseAsync(context);
        var alpha = CreateUser(admin, warehouse, "alpha", UserStatus.Active, dataLimit: 5);
        alpha.Connections.Add(new ConnectionEntity { Password = "password-1" });
        alpha.Connections.Add(new ConnectionEntity { Password = "password-2" });
        context.Users.Add(alpha);
        context.Users.Add(CreateUser(admin, warehouse, "disabled", UserStatus.Disabled, dataLimit: 20));
        context.Users.Add(CreateUser(admin, warehouse, "gamma", UserStatus.Limited, dataLimit: 30));
        await context.SaveChangesAsync();
        var repository = new UserRepository(context);

        var page = await repository.SearchAsync(
            admin.Id,
            new UserFilter { SortBy = sortBy, SortOrder = sortOrder },
            CancellationToken.None);

        page.Items[0].Username.Should().Be(expectedFirstUsername);
    }

    [Fact]
    public async Task SearchAsync_DefaultSortsByCreatedAtDescending()
    {
        await using var context = CreateContext();
        var (admin, warehouse) = await CreateAdminWithWarehouseAsync(context);
        var oldUser = CreateUser(admin, warehouse, "old", UserStatus.Active);
        oldUser.CreatedAt = DateTimeOffset.UtcNow.AddDays(-2);
        var newUser = CreateUser(admin, warehouse, "new", UserStatus.Active);
        newUser.CreatedAt = DateTimeOffset.UtcNow;
        context.Users.Add(oldUser);
        context.Users.Add(newUser);
        await context.SaveChangesAsync();
        var repository = new UserRepository(context);

        var page = await repository.SearchAsync(admin.Id, new UserFilter(), CancellationToken.None);

        page.Items[0].Username.Should().Be("new");
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql(fixture.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<(AdminAccount Admin, WarehouseEntity Warehouse)> CreateAdminWithWarehouseAsync(
        AppDbContext context)
    {
        var admin = new AdminAccount
        {
            Username = $"admin-{Guid.NewGuid():N}",
            PasswordHash = "hash",
            Permissions = AdminPermission.SuperAdmin
        };
        var warehouse = new WarehouseEntity
        {
            Name = "Warehouse",
            Note = string.Empty,
            Enabled = true,
            Admin = admin
        };

        context.AdminAccounts.Add(admin);
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        return (admin, warehouse);
    }

    private static UserEntity CreateUser(
        AdminAccount admin,
        WarehouseEntity warehouse,
        string username,
        UserStatus status,
        ulong dataLimit = 0)
    {
        return new UserEntity
        {
            Username = username,
            Status = status,
            DataLimit = dataLimit,
            ConnectionLimit = 2,
            Admin = admin,
            Warehouse = warehouse
        };
    }
}
