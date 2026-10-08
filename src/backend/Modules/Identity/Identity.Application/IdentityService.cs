using Identity.Contracts;

namespace Identity.Application;

public sealed class IdentityService(IAccountStore accounts, ITokenIssuer tokens) : IIdentityService
{
    public Task<RegistrationResult> RegisterAsync(RegisterRequest request) => accounts.CreateAsync(request);

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await accounts.ValidateCredentialsAsync(request.Email, request.Password);
        return user is null ? null : tokens.Issue(user);
    }

    public Task<UserProfile?> GetProfileAsync(string userId) => accounts.FindAsync(userId);
}
