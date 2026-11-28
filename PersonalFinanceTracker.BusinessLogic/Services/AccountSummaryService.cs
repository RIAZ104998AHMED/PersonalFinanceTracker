using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public sealed class AccountSummaryService
    : IAccountSummaryService
{
    private readonly FinanceTrackerDbContext _dbContext;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IConfiguration _configuration;

    public AccountSummaryService(
        FinanceTrackerDbContext dbContext,
        IExchangeRateService exchangeRateService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _exchangeRateService = exchangeRateService;
        _configuration = configuration;
    }

    public async Task<AccountSummaryDto> GetSummaryAsync(
        Guid userId,
        string currency,
        DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new BadRequestException(
                "Currency is required.");
        }

        var targetCurrency = currency.Trim();

        // Keep Project 1 currency validation behaviour:
        // lowercase values are not silently normalized.
        if (targetCurrency != targetCurrency.ToUpperInvariant())
        {
            throw new BadRequestException(
                "Currency must be uppercase.");
        }

        var allowedCurrencies =
            _configuration
                .GetSection("Currencies:Allowed")
                .Get<string[]>()
            ?? [];

        var allowedSet =
            allowedCurrencies
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.Ordinal);

        if (!allowedSet.Contains(targetCurrency))
        {
            throw new BadRequestException(
                "Currency is not supported.");
        }

        var requestedDate =
            date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        // This also handles future-date validation
        // and obtaining/caching Bank of Russia rates.
        var ratesResponse =
            await _exchangeRateService.GetRatesAsync(
                requestedDate,
                cancellationToken);

        var rates =
            ratesResponse.Rates
                .ToDictionary(
                    r => r.Currency,
                    r => r.Rate,
                    StringComparer.Ordinal);

        if (!rates.TryGetValue(
                targetCurrency,
                out var targetRate))
        {
            throw new ServiceUnavailableException(
                $"Exchange rate for {targetCurrency} is unavailable.");
        }

        var accounts =
            await _dbContext.Accounts
                .AsNoTracking()
                .Where(a =>
                    a.UserId == userId &&
                    !a.IsArchived)
                .OrderBy(a => a.CreatedAt)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Currency,

                    Balance =
                        a.StartAmount

                        + (_dbContext.Operations
                            .Where(o =>
                                o.AccountId == a.Id &&
                                o.Type ==
                                    OperationType.Income)
                            .Sum(o =>
                                (decimal?)o.Amount)
                            ?? 0m)

                        - (_dbContext.Operations
                            .Where(o =>
                                o.AccountId == a.Id &&
                                o.Type ==
                                    OperationType.Expense)
                            .Sum(o =>
                                (decimal?)o.Amount)
                            ?? 0m)

                        + (_dbContext.Transfers
                            .Where(t =>
                                t.ToAccountId == a.Id)
                            .Sum(t =>
                                (decimal?)t.Amount)
                            ?? 0m)

                        - (_dbContext.Transfers
                            .Where(t =>
                                t.FromAccountId == a.Id)
                            .Sum(t =>
                                (decimal?)t.Amount)
                            ?? 0m)
                })
                .ToListAsync(cancellationToken);

        var resultAccounts =
            new List<AccountSummaryItemDto>();

        foreach (var account in accounts)
        {
            if (!rates.TryGetValue(
                    account.Currency,
                    out var sourceRate))
            {
                throw new ServiceUnavailableException(
                    $"Exchange rate for {account.Currency} is unavailable.");
            }

            var convertedBalance =
                Math.Round(
                    account.Balance *
                    sourceRate /
                    targetRate,
                    2,
                    MidpointRounding.AwayFromZero);

            // Matches the assignment response example.
            var displayedRate =
                sourceRate == targetRate
                    ? 1m
                    : targetRate / sourceRate;

            resultAccounts.Add(
                new AccountSummaryItemDto(
                    account.Id,
                    account.Name,
                    account.Currency,
                    account.Balance,
                    displayedRate,
                    convertedBalance));
        }

        var total =
            resultAccounts.Sum(
                x => x.ConvertedBalance);

        return new AccountSummaryDto(
            targetCurrency,
            requestedDate,
            total,
            resultAccounts);
    }
}