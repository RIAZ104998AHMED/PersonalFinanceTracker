using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public sealed class ExchangeRateService
    : IExchangeRateService
{
    private readonly FinanceTrackerDbContext _dbContext;
    private readonly IExchangeRateClient _exchangeRateClient;
    private readonly IConfiguration _configuration;

    public ExchangeRateService(
        FinanceTrackerDbContext dbContext,
        IExchangeRateClient exchangeRateClient,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _exchangeRateClient = exchangeRateClient;
        _configuration = configuration;
    }

    public async Task<ExchangeRatesDto> GetRatesAsync(
        DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        var requestedDate =
            date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var currentDate =
            DateOnly.FromDateTime(DateTime.UtcNow);

        if (requestedDate > currentDate)
        {
            throw new BadRequestException(
                "Exchange rates cannot be requested for a future date.");
        }

        var allowedCurrencies =
            _configuration
                .GetSection("Currencies:Allowed")
                .Get<string[]>()
            ?? [];

        var allowedSet =
            allowedCurrencies
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(
                    StringComparer.Ordinal);

        // RUB is returned as 1 but is never stored.
        allowedSet.Remove("RUB");

        var storedRates =
            await _dbContext.ExchangeRates
                .AsNoTracking()
                .Where(r =>
                    r.Date == requestedDate)
                .OrderBy(r => r.Currency)
                .ToListAsync(cancellationToken);

        var hasAllStoredRates =
            allowedSet.Count > 0 &&
            allowedSet.All(currency =>
                storedRates.Any(r =>
                    r.Currency == currency));

        if (!hasAllStoredRates)
        {
            IReadOnlyCollection<
                BusinessLogic.Models.ExchangeRateValue> externalRates;

            try
            {
                externalRates =
                    await _exchangeRateClient
                        .GetRatesAsync(
                            requestedDate,
                            cancellationToken);
            }
            catch (Exception)
            {
                throw new ServiceUnavailableException(
                    "The Bank of Russia exchange-rate service is unavailable.");
            }

            var filteredRates =
                externalRates
                    .Where(r =>
                        allowedSet.Contains(
                            r.Currency))
                    .ToList();

            if (!allowedSet.All(currency =>
                    filteredRates.Any(r =>
                        r.Currency == currency)))
            {
                throw new ServiceUnavailableException(
                    "The Bank of Russia response does not contain all required currencies.");
            }

            var existingCurrencies =
                storedRates
                    .Select(r => r.Currency)
                    .ToHashSet(
                        StringComparer.Ordinal);

            foreach (var rate in filteredRates)
            {
                if (existingCurrencies.Contains(
                        rate.Currency))
                {
                    continue;
                }

                _dbContext.ExchangeRates.Add(
                    new ExchangeRate
                    {
                        Date = requestedDate,
                        Currency = rate.Currency,
                        Rate = rate.Rate
                    });
            }

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            storedRates =
                await _dbContext.ExchangeRates
                    .AsNoTracking()
                    .Where(r =>
                        r.Date == requestedDate)
                    .OrderBy(r => r.Currency)
                    .ToListAsync(
                        cancellationToken);
        }

        var responseRates =
            storedRates
                .Where(r =>
                    allowedSet.Contains(
                        r.Currency))
                .Select(r =>
                    new ExchangeRateDto(
                        r.Currency,
                        r.Rate))
                .ToList();

        responseRates.Add(
            new ExchangeRateDto(
                "RUB",
                1.000000m));

        responseRates =
            responseRates
                .OrderBy(r => r.Currency)
                .ToList();

        return new ExchangeRatesDto(
            requestedDate,
            "RUB",
            responseRates);
    }
}