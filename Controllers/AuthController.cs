using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request)
    {
        var result =
            await _authService.RegisterAsync(request);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Registration failed",
                detail: "A user with this email already exists or registration failed.");
        }

        return Created(
            "/api/users/me",
            result);
    }

    // POST /api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request)
    {
        var result =
            await _authService.LoginAsync(request);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: "Invalid email or password.");
        }

        return Ok(result);
    }

    // BONUS B1
    // POST /api/auth/refresh
    [Authorize]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshTokenRequest request)
    {
        var result =
            await _authService.RefreshAsync(request);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Refresh failed",
                detail: "The refresh token is invalid, expired, or revoked.");
        }

        return Ok(result);
    }

    // BONUS B2
    // POST /api/auth/logout
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "A valid access token is required.");
        }

        var success =
            await _authService.LogoutAsync(
                userId.Value,
                request);

        if (!success)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Logout failed",
                detail: "The refresh token is invalid, expired, revoked, or does not belong to the current user.");
        }

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var value =
            User.FindFirst(
                JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(
            value,
            out var userId)
            ? userId
            : null;
    }
}