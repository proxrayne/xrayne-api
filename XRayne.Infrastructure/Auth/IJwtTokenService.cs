using XRayne.Core.Auth;

namespace XRayne.Infrastructure.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid adminId, string username, AdminPermission permissions);
}
