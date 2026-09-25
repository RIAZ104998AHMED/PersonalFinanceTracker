namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public record AccountDto(
    Guid Id,
    string Name,
    decimal Balance
);