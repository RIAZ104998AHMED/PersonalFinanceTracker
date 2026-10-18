using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid userId);

    Task<UserDto?> UpdateNameAsync(
        Guid userId,
        UpdateUserRequest request);
}