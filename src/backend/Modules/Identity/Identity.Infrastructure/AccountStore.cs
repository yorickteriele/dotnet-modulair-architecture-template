using Identity.Application;
using Identity.Contracts;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Identity.Infrastructure;

public sealed class AccountStore(UserManager<ApplicationUser> users) : IAccountStore
{
    public async Task<RegistrationResult> CreateAsync(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email, DisplayName = request.DisplayName };
        try
        {
            var result = await users.CreateAsync(user, request.Password);
            return result.Succeeded
                ? new RegistrationResult(Profile(user), [])
                : new RegistrationResult(null, result.Errors.Select(e => e.Description).ToArray());
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return new RegistrationResult(null, ["An account with this email already exists."]);
        }
    }

    public async Task<UserProfile?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null || await users.IsLockedOutAsync(user)) return null;
        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            return null;
        }
        await users.ResetAccessFailedCountAsync(user);
        return Profile(user);
    }

    public async Task<UserProfile?> FindAsync(string userId)
    {
        var user = await users.FindByIdAsync(userId);
        return user is null ? null : Profile(user);
    }

    private static UserProfile Profile(ApplicationUser user) => new(user.Id, user.Email!, user.DisplayName);
}
