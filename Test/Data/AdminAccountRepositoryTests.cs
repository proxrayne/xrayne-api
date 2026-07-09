using Data;
using Microsoft.EntityFrameworkCore;
using Contracts.Enums;
using Data.Entities;

namespace Test.Data;

/// <summary>
/// Tests administrator account persistence.
/// </summary>
public sealed class AdminAccountRepositoryTests
{
    [Fact]
    public void Model_ConfiguresAdminSoftDeleteDefault()
    {
        using var context = CreateNpgsqlModelContext();

        context.Model.FindEntityType(typeof(AdminAccount))!
            .FindProperty(nameof(AdminAccount.IsDeleted))!
            .GetDefaultValue()
            .Should()
            .Be(false);
    }

    [Fact]
    public void Model_DoesNotFilterSoftDeletedAdmins()
    {
        using var context = CreateNpgsqlModelContext();

        context.Model.FindEntityType(typeof(AdminAccount))!
            .GetQueryFilter()
            .Should()
            .BeNull();
    }

    [Fact]
    public void Model_KeepsAdminNavigationRequired()
    {
        using var context = CreateNpgsqlModelContext();

        var foreignKey = context.Model.FindEntityType(typeof(NodeEntity))!
            .FindNavigation(nameof(NodeEntity.Admin))!
            .ForeignKey;

        foreignKey.IsRequired.Should().BeTrue();
        foreignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    [Fact]
    public void Permissions_KeepsSuperAdminOnStableHighBit()
    {
        ((long)AdminPermission.SuperAdmin).Should().Be(1L << 62);
    }

    private static AppDbContext CreateNpgsqlModelContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql("Host=localhost;Database=xrayne;Username=xrayne;Password=xrayne")
            .Options;

        return new AppDbContext(options);
    }
}
