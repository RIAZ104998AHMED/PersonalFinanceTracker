using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var user =
            await _userService.GetByIdAsync(userId.Value);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(
        [FromBody] UpdateUserRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var user =
            await _userService.UpdateNameAsync(
                userId.Value,
                request);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    private Guid? GetCurrentUserId()
    {
        var sub =
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(sub, out var userId)
            ? userId
            : null;
    }
}