using Identity.Contracts;

namespace Identity.Application;

public interface IAccountStore
{
    Task<RegistrationResult> CreateAsync(RegisterRequest request);
    Task<UserProfile?> ValidateCredentialsAsync(string email, string password);
    Task<UserProfile?> FindAsync(string userId);
}
public interface ITokenIssuer
{
    AuthResponse Issue(UserProfile user);
}
