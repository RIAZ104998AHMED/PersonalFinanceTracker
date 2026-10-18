using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;

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

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Registration failed",
                detail: "A user with this email already exists or the registration data is invalid."
            );
        }

        return Created("/api/users/me", result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: "Invalid email or password."
            );
        }

        return Ok(result);
    }
}