using Identity.Application;
using Identity.Contracts;
using Moq;
using NUnit.Framework;

namespace Identity.Tests;

public sealed class IdentityServiceTests
{
    [Test]
    public async Task Login_InvalidCredentials_DoesNotIssueToken()
    {
        var accounts = new Mock<IAccountStore>();
        var tokens = new Mock<ITokenIssuer>(MockBehavior.Strict);
        var service = new IdentityService(accounts.Object, tokens.Object);
        Assert.That(await service.LoginAsync(new("nobody@example.com", "wrong")), Is.Null);
        tokens.Verify(t => t.Issue(It.IsAny<UserProfile>()), Times.Never);
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsIssuedToken()
    {
        var user = new UserProfile("id", "user@example.com", "User");
        var response = new AuthResponse("token", DateTimeOffset.UtcNow.AddMinutes(30), user);
        var accounts = new Mock<IAccountStore>();
        accounts.Setup(a => a.ValidateCredentialsAsync(user.Email, "password")).ReturnsAsync(user);
        var tokens = new Mock<ITokenIssuer>();
        tokens.Setup(t => t.Issue(user)).Returns(response);
        Assert.That(await new IdentityService(accounts.Object, tokens.Object).LoginAsync(new(user.Email, "password")), Is.EqualTo(response));
    }

    [Test]
    public async Task Register_FailedStore_ReturnsErrors()
    {
        var request = new RegisterRequest("user@example.com", "weak", "User");
        var accounts = new Mock<IAccountStore>();
        accounts.Setup(a => a.CreateAsync(request)).ReturnsAsync(new RegistrationResult(null, ["Password too short"]));
        var result = await new IdentityService(accounts.Object, Mock.Of<ITokenIssuer>()).RegisterAsync(request);
        Assert.That(result.User, Is.Null);
        Assert.That(result.Errors, Is.EqualTo(new[] { "Password too short" }));
    }
}
