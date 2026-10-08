using Microsoft.AspNetCore.Http;
using Identity.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.Api;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = "identity")]
[Route("api/v1/Identity/auth")]
public sealed class AuthController(IIdentityService identity) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType<UserProfile>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfile>> Register(RegisterRequest request)
    {
        var result = await identity.RegisterAsync(request);
        if (result.User is null)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["registration"] = result.Errors }));
        return Created("/api/v1/Identity/auth/me", result.User);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await identity.LoginAsync(request);
        return result is null ? Unauthorized() : Ok(result);
    }

    [HttpGet("me")]
    [ProducesResponseType<UserProfile>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfile>> Me()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId is null) return Unauthorized();
        var profile = await identity.GetProfileAsync(userId);
        return profile is null ? Unauthorized() : Ok(profile);
    }
}
