using Contracts.Enums;

namespace Infrastructure.Services;

public interface IJwtTokenService
{
    string CreateAccessToken(
        Guid adminId,
        string username,
        AdminPermission permissions,
        int? lifetimeMinutes = null);
}
