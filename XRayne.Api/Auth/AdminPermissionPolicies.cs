using Microsoft.AspNetCore.Authorization;
using XRayne.Core.Auth;

namespace XRayne.Api.Auth;

public static class AdminPermissionPolicies
{
    public static void AddAdminPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(AdminPermissionNames.SuperAdmin, policy =>
            policy.RequireAssertion(context => HasPermission(context, AdminPermissionNames.SuperAdmin)));

        AddPermissionPolicy(options, AdminPermissionNames.CreateUsers);
        AddPermissionPolicy(options, AdminPermissionNames.EditUsers);
        AddPermissionPolicy(options, AdminPermissionNames.DeleteUsers);
        AddPermissionPolicy(options, AdminPermissionNames.ResetTraffic);
        AddPermissionPolicy(options, AdminPermissionNames.ChangeXraySettings);
        AddPermissionPolicy(options, AdminPermissionNames.ViewLogs);
        AddPermissionPolicy(options, AdminPermissionNames.ManageAdmins);
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
