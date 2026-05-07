using XRayne.Contracts.Enums;

namespace XRayne.Infrastructure.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid adminId, string username, AdminPermission permissions);
}
