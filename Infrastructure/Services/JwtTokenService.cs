using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Enums;
using XRayne.Contracts.Values;

namespace XRayne.Infrastructure.Services;

/// <summary>
/// Creates JWT access tokens for panel administrators.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public string CreateAccessToken(
        Guid adminId,
        string username,
        AdminPermission permissions,
        int? lifetimeMinutes = null)
    {
        var jwtOptions = options.Value;
        if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT secret is not configured.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, adminId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.NameIdentifier, adminId.ToString()),
            new(ClaimTypes.Name, username)
        };

        foreach (var permission in AdminPermissionNames.ToNames(permissions))
        {
            claims.Add(new Claim("permission", permission));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes ?? jwtOptions.AccessTokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
