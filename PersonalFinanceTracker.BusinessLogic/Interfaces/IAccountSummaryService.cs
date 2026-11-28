using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IAccountSummaryService
{
    Task<AccountSummaryDto> GetSummaryAsync(
        Guid userId,
        string currency,
        DateOnly? date,
        CancellationToken cancellationToken = default);
}