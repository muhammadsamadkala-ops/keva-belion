using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KivaBelion.API.Controllers;

[ApiController]
[Route("api/v1/admin/auth")]
public sealed class AdminAuthController(AdminAuthService auth, ITokenService tokens) : ControllerBase
{
    /// <summary>Signs in a staff member and returns a short-lived access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.LoginAsync(request.Email, request.Password, cancellationToken);
        if (!result.Succeeded)
        {
            // Same answer for every kind of failure, so attackers learn nothing.
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");
        }

        var token = tokens.CreateAccessToken(result.User!);
        return Ok(new LoginResponse(token.Token, token.ExpiresAtUtc, result.User!.DisplayName, result.User.Role.ToString()));
    }

    /// <summary>Who am I? Requires a valid token.</summary>
    [HttpGet("me")]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public ActionResult<MeResponse> Me() =>
        Ok(new MeResponse(User.FindFirstValue("email") ?? "", User.FindFirstValue("name") ?? "", User.FindFirstValue("role") ?? ""));

    /// <summary>Proves role checks work: only Admin accounts get through.</summary>
    [HttpGet("admin-check")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public IActionResult AdminCheck() => Ok(new { Message = "You are an Admin." });
}

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, string DisplayName, string Role);
public sealed record MeResponse(string Email, string DisplayName, string Role);
