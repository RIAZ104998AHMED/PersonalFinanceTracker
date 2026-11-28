namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public sealed record AccountSummaryItemDto(
    Guid AccountId,
    string Name,
    string Currency,
    decimal Balance,
    decimal Rate,
    decimal ConvertedBalance);