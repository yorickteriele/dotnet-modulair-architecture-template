using System.Text;
using Identity.Application;
using Identity.Contracts;
using Identity.Domain;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Module.Abstractions;

namespace Identity.Api;

public sealed class IdentityModule : DbContextModuleBase<IdentityDbContext>
{
    public override string ApiGroupName => "identity";
    protected override string Schema => IdentityDbContext.Schema;

    protected override void RegisterModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        var jwt = section.Get<JwtOptions>() ?? new JwtOptions();
        if (Encoding.UTF8.GetByteCount(jwt.Secret) < 32 || string.IsNullOrWhiteSpace(jwt.Issuer)
            || string.IsNullOrWhiteSpace(jwt.Audience) || jwt.LifetimeMinutes is < 1 or > 1440)
            throw new InvalidOperationException("Configure Jwt with a 32-byte minimum secret, issuer, audience and lifetime of 1-1440 minutes.");
        services.Configure<JwtOptions>(section);
        services.AddSingleton(TimeProvider.System);
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        }).AddEntityFrameworkStores<IdentityDbContext>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        services.AddAuthorization();
        services.AddScoped<IAccountStore, AccountStore>();
        services.AddScoped<ITokenIssuer, TokenIssuer>();
        services.AddScoped<IIdentityService, IdentityService>();
    }
}
