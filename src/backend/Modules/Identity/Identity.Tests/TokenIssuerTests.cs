using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Identity.Contracts;
using Identity.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace Identity.Tests;

public sealed class TokenIssuerTests
{
    [Test]
    public void Issue_ValidUser_CreatesSignedExpiringToken()
    {
        var options = new JwtOptions { Secret = new string('a', 64), Issuer = "test", Audience = "test", LifetimeMinutes = 5 };
        var response = new TokenIssuer(Options.Create(options), TimeProvider.System).Issue(new UserProfile("user-id", "user@example.com", "User"));
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(response.AccessToken, new TokenValidationParameters
        {
            ValidIssuer = options.Issuer, ValidAudience = options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)),
            ValidateLifetime = true, ValidateIssuerSigningKey = true, ClockSkew = TimeSpan.Zero
        }, out var validated);
        Assert.That(principal.FindFirst("sub")?.Value, Is.EqualTo("user-id"));
        Assert.That(validated.ValidTo, Is.EqualTo(response.ExpiresAt.UtcDateTime).Within(TimeSpan.FromSeconds(1)));
        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() => handler.ValidateToken(response.AccessToken,
            new TokenValidationParameters { ValidIssuer = options.Issuer, ValidAudience = options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new string('b', 64))) }, out _));
    }
}
