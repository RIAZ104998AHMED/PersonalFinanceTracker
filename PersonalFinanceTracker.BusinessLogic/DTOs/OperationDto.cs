using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record OperationDto(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    OperationType Type,
    decimal Amount,
    DateOnly Date,
    string? Comment,
    DateTime CreatedAt
);