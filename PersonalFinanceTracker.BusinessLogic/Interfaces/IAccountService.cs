using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAsync(
        Guid userId,
        bool includeArchived = false);

    Task<AccountDto?> GetByIdAsync(
        Guid userId,
        Guid id);

    Task<AccountDto> CreateAsync(
        Guid userId,
        CreateAccountRequest request);
}