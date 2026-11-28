namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public sealed record AccountSummaryDto(
    string Currency,
    DateOnly Date,
    decimal Total,
    IReadOnlyCollection<AccountSummaryItemDto> Accounts);