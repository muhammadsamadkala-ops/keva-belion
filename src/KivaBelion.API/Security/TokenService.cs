using KivaBelion.API.Data.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace KivaBelion.API.Security;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAtUtc);

public interface ITokenService
{
    AccessToken CreateAccessToken(AdminUser user);
}

/// <summary>Creates short-lived signed access tokens (JWT, HMAC-SHA256). The token holds no password and no secrets.</summary>
public sealed class TokenService(IOptions<JwtSettings> options, TimeProvider time) : ITokenService
{
    public AccessToken CreateAccessToken(AdminUser user)
    {
        var settings = options.Value;
        var now = time.GetUtcNow();
        var expires = now.AddMinutes(settings.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = user.Id.ToString(),
                ["email"] = user.Email,
                ["name"] = user.DisplayName,
                ["role"] = user.Role.ToString(),
                ["jti"] = Guid.NewGuid().ToString("N")
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(settings.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}
