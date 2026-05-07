using XRayne.Contracts.Enums;

namespace XRayne.Infrastructure.Services;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid adminId, string username, AdminPermission permissions);
}
