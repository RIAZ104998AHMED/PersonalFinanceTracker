using PersonalFinanceTracker.BusinessLogic.DTOs;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IExchangeRateService
{
    Task<ExchangeRatesDto> GetRatesAsync(
        DateOnly? date,
        CancellationToken cancellationToken = default);
}