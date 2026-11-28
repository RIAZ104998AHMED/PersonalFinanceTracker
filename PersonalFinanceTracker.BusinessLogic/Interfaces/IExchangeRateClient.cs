using PersonalFinanceTracker.BusinessLogic.Models;

namespace PersonalFinanceTracker.BusinessLogic.Interfaces;

public interface IExchangeRateClient
{
    Task<IReadOnlyCollection<ExchangeRateValue>>
        GetRatesAsync(
            DateOnly date,
            CancellationToken cancellationToken = default);
}