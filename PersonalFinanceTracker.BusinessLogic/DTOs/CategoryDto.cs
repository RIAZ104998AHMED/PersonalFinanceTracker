using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    OperationType Type,
    bool IsSystem
);