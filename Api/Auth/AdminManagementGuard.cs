using Contracts.Enums;
using Contracts.Exceptions;

namespace Api.Auth;

/// <summary>
/// Validates administrator management access rules that are stricter than the authorization policy.
/// </summary>
public static class AdminManagementGuard
{
    /// <summary>
    /// Ensures a caller can manage a target account and requested permissions.
    /// </summary>
    public static void EnsureCanManage(
        AdminPermission callerPermissions,
        AdminPermission targetPermissions,
        AdminPermission? requestedPermissions = null)
    {
        if (callerPermissions.HasFlag(AdminPermission.SuperAdmin))
        {
            return;
        }

        if (targetPermissions.HasFlag(AdminPermission.SuperAdmin)
            || requestedPermissions?.HasFlag(AdminPermission.SuperAdmin) == true)
        {
            throw new ForbiddenException("Only super administrators can manage super administrator accounts.");
        }
    }
}
