using Microsoft.AspNetCore.Identity;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        return MapToDto(user);
    }

    public async Task<UserDto?> UpdateNameAsync(
        Guid userId,
        UpdateUserRequest request)
    {
        var user =
            await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        user.Name = request.Name.Trim();

        var result =
            await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return null;
        }

        return MapToDto(user);
    }

    private static UserDto MapToDto(
        ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Name = user.Name,
            CreatedAt = user.CreatedAt
        };
    }
}