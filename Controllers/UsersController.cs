using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.Api.Services;

namespace PersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    private readonly ICurrentUser _currentUser;

    public UsersController(IUserService userService, ICurrentUser currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var user =
            await _userService.GetByIdAsync(userId);

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
        if (_currentUser.UserId is not Guid userId)
        {
            return Unauthorized();
        }

        var user =
            await _userService.UpdateNameAsync(
                userId,
                request);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

}