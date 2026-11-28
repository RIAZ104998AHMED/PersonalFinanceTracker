namespace PersonalFinanceTracker.BusinessLogic.DTOs;

public sealed record ExchangeRatesDto(
    DateOnly Date,
    string Base,
    IReadOnlyCollection<ExchangeRateDto> Rates);