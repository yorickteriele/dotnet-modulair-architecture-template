using System.ComponentModel.DataAnnotations;

namespace Identity.Contracts;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(12), MaxLength(128)] string Password,
    [Required, MaxLength(200)] string DisplayName);
public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record UserProfile([property: Required] string Id, [property: Required] string Email, [property: Required] string DisplayName);
public sealed record AuthResponse([property: Required] string AccessToken, [property: Required] DateTimeOffset ExpiresAt, [property: Required] UserProfile User);
public sealed record RegistrationResult(UserProfile? User, string[] Errors);
public interface IIdentityService
{
    Task<RegistrationResult> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<UserProfile?> GetProfileAsync(string userId);
}
