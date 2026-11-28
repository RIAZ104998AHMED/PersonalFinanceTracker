namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public sealed record ExchangeRateDto(
    string Currency,
    decimal Rate);