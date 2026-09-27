namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record AccountDto(
    Guid Id,
    string Name,
    string Currency,
    decimal StartAmount,
    bool IsArchived,
    DateTime CreatedAt,
    decimal Balance
);