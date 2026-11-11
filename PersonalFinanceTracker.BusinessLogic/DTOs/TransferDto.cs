namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record TransferDto(
    Guid Id,
    Guid FromAccountId,
    string FromAccountName,
    Guid ToAccountId,
    string ToAccountName,
    string Currency,
    decimal Amount,
    DateOnly Date,
    string? Comment,
    DateTime CreatedAt
);