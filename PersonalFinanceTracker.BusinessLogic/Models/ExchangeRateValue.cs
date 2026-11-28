namespace PersonalFinanceTracker.BusinessLogic.Models;

public sealed record ExchangeRateValue(
    string Currency,
    decimal Rate);