using Api.Auth;
using Contracts.Enums;
using Contracts.Exceptions;

namespace Test.Auth;

/// <summary>
/// Tests administrator management guard rules.
/// </summary>
public sealed class AdminManagementGuardTests
{
    [Fact]
    public void EnsureCanManage_AllowsSuperAdminToManageSuperAdmin()
    {
        var act = () => AdminManagementGuard.EnsureCanManage(
            AdminPermission.SuperAdmin,
            AdminPermission.SuperAdmin,
            AdminPermission.SuperAdmin);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanManage_AllowsManagerToManageNonSuperAdmin()
    {
        var act = () => AdminManagementGuard.EnsureCanManage(
            AdminPermission.ManageAdmins,
            AdminPermission.ManageWarehouses,
            AdminPermission.ManageAdmins);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanManage_BlocksManagerFromManagingSuperAdmin()
    {
        var act = () => AdminManagementGuard.EnsureCanManage(
            AdminPermission.ManageAdmins,
            AdminPermission.SuperAdmin);

        act.Should().Throw<ForbiddenException>();
    }

    [Fact]
    public void EnsureCanManage_BlocksManagerFromAssigningSuperAdmin()
    {
        var act = () => AdminManagementGuard.EnsureCanManage(
            AdminPermission.ManageAdmins,
            AdminPermission.ManageAdmins,
            AdminPermission.SuperAdmin);

        act.Should().Throw<ForbiddenException>();
    }
}
