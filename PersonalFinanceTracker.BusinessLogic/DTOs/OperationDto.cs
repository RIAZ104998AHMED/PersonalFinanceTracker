using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record OperationDto(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string Currency,
    Guid CategoryId,
    string CategoryName,
    OperationType Type,
    decimal Amount,
    DateOnly Date,
    string? Comment,
    DateTime CreatedAt
);