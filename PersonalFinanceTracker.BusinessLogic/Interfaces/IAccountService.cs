using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> GetAllAsync(
        bool includeArchived = false);

    Task<AccountDto> CreateAsync(
        CreateAccountRequest request);
}