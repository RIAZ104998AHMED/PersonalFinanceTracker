namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record TransferDto(
    Guid Id,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateOnly Date,
    string? Comment,
    DateTime CreatedAt
);