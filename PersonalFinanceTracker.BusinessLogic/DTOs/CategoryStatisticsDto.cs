using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record CategoryStatisticsDto(
    Guid CategoryId,
    string CategoryName,
    OperationType Type,
    decimal Amount,
    decimal Share
);