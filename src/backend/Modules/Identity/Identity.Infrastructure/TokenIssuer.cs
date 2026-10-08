using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Application;
using Identity.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure;

public sealed class TokenIssuer(IOptions<JwtOptions> options, TimeProvider clock) : ITokenIssuer
{
    public AuthResponse Issue(UserProfile user)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        var expires = now.AddMinutes(settings.LifetimeMinutes);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now.UtcDateTime, expires.UtcDateTime,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)), SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, user);
    }
}
