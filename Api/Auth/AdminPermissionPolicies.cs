using Microsoft.AspNetCore.Authorization;
using Contracts.Values;

namespace Api.Auth;

/// <summary>
/// Registers authorization policies for administrator permissions.
/// </summary>
public static class AdminPermissionPolicies
{
    /// <summary>
    /// Allows reading users when any user-management permission is granted.
    /// </summary>
    public const string ReadUsers = "read_users";

    /// <summary>
    /// Adds administrator permission policies.
    /// </summary>
    public static void AddAdminPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(AdminPermissionNames.SuperAdmin, policy =>
            policy.RequireAssertion(context => HasPermission(context, AdminPermissionNames.SuperAdmin)));
        options.AddPolicy(ReadUsers, policy =>
            policy.RequireAssertion(context =>
                HasPermission(context, AdminPermissionNames.SuperAdmin) ||
                HasPermission(context, AdminPermissionNames.CreateUsers) ||
                HasPermission(context, AdminPermissionNames.EditUsers) ||
                HasPermission(context, AdminPermissionNames.DeleteUsers) ||
                HasPermission(context, AdminPermissionNames.ResetTraffic)));

        AddPermissionPolicy(options, AdminPermissionNames.CreateUsers);
        AddPermissionPolicy(options, AdminPermissionNames.EditUsers);
        AddPermissionPolicy(options, AdminPermissionNames.DeleteUsers);
        AddPermissionPolicy(options, AdminPermissionNames.ResetTraffic);
        AddPermissionPolicy(options, AdminPermissionNames.ChangeXraySettings);
        AddPermissionPolicy(options, AdminPermissionNames.ViewLogs);
        AddPermissionPolicy(options, AdminPermissionNames.ManageAdmins);
        AddPermissionPolicy(options, AdminPermissionNames.ManageWarehouses);
    }

    private static void AddPermissionPolicy(AuthorizationOptions options, string permission)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireAssertion(context =>
                HasPermission(context, AdminPermissionNames.SuperAdmin) ||
                HasPermission(context, permission)));
    }

    private static bool HasPermission(AuthorizationHandlerContext context, string permission)
    {
        return context.User.HasClaim("permission", permission);
    }
}
