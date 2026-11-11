using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class StatisticsService : IStatisticsService
{
    private readonly FinanceTrackerDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public StatisticsService(
        FinanceTrackerDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<StatisticsDto> GetAsync(
        Guid userId,
        StatisticsQueryParameters parameters)
    {
        ValidateParameters(parameters);

        var from = parameters.From!.Value;
        var to = parameters.To!.Value;

        var groupBy =
            string.IsNullOrWhiteSpace(parameters.GroupBy)
                ? "day"
                : parameters.GroupBy
                    .Trim()
                    .ToLowerInvariant();

        List<Guid> accountIds;

        Guid? resultAccountId = null;
        string? resultCurrency = null;

        // R16 - statistics for one account
        if (parameters.AccountId.HasValue)
        {
            var account =
                await _dbContext.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        a =>
                            a.Id ==
                            parameters.AccountId.Value);

            if (account is null)
            {
                throw new NotFoundException(
                    "Account was not found.");
            }

            // R15
            if (account.UserId != userId)
            {
                throw new ForbiddenException(
                    "You do not have access to this account.");
            }

            accountIds =
                new List<Guid>
                {
                    account.Id
                };

            resultAccountId = account.Id;

            // Required by API contract:
            // statistics for an account must return
            // that account's currency.
            resultCurrency = account.Currency;
        }
        else
        {
            // R16 - statistics for all non-archived
            // accounts of one currency
            var currency =
                parameters.Currency!
                    .Trim()
                    .ToUpperInvariant();

            ValidateCurrency(currency);

            accountIds =
                await _dbContext.Accounts
                    .AsNoTracking()
                    .Where(a =>
                        a.UserId == userId &&
                        !a.IsArchived &&
                        a.Currency == currency)
                    .Select(a => a.Id)
                    .ToListAsync();

            resultCurrency = currency;
        }

        // No accounts of this currency
        if (accountIds.Count == 0)
        {
            return CreateEmptyResult(
                from,
                to,
                resultCurrency,
                resultAccountId);
        }

        // Statistics use operations only.
        // Transfers are excluded.
        var operations =
            await _dbContext.Operations
                .AsNoTracking()
                .Where(o =>
                    accountIds.Contains(o.AccountId) &&
                    o.Date >= from &&
                    o.Date <= to)
                .ToListAsync();

        if (operations.Count == 0)
        {
            return CreateEmptyResult(
                from,
                to,
                resultCurrency,
                resultAccountId);
        }

        var totalIncome =
            operations
                .Where(o =>
                    o.Type ==
                    OperationType.Income)
                .Sum(o => o.Amount);

        var totalExpense =
            operations
                .Where(o =>
                    o.Type ==
                    OperationType.Expense)
                .Sum(o => o.Amount);

        var net =
            totalIncome - totalExpense;

        // Load category names
        var categoryIds =
            operations
                .Select(o => o.CategoryId)
                .Distinct()
                .ToList();

        var categories =
            await _dbContext.Categories
                .AsNoTracking()
                .Where(c =>
                    categoryIds.Contains(c.Id))
                .ToDictionaryAsync(
                    c => c.Id,
                    c => c.Name);

        // Category breakdown
        var byCategory =
            operations
                .GroupBy(o => new
                {
                    o.CategoryId,
                    o.Type
                })
                .Select(group =>
                {
                    var amount =
                        group.Sum(
                            o => o.Amount);

                    var typeTotal =
                        group.Key.Type ==
                        OperationType.Income
                            ? totalIncome
                            : totalExpense;

                    var share =
                        typeTotal == 0m
                            ? 0m
                            : Math.Round(
                                amount / typeTotal,
                                4,
                                MidpointRounding
                                    .AwayFromZero);

                    var categoryName =
                        categories.TryGetValue(
                            group.Key.CategoryId,
                            out var name)
                            ? name
                            : "Unknown";

                    return new CategoryStatisticsDto(
                        group.Key.CategoryId,
                        categoryName,
                        group.Key.Type,
                        amount,
                        share);
                })
                .OrderByDescending(
                    x => x.Amount)
                .ToList();

        IEnumerable<TimelineStatisticsDto> timeline;

        if (groupBy == "month")
        {
            timeline =
                operations
                    .GroupBy(o => new
                    {
                        o.Date.Year,
                        o.Date.Month
                    })
                    .OrderBy(
                        group =>
                            group.Key.Year)
                    .ThenBy(
                        group =>
                            group.Key.Month)
                    .Select(group =>
                        new TimelineStatisticsDto(
                            $"{group.Key.Year:D4}-{group.Key.Month:D2}",
                            group
                                .Where(o =>
                                    o.Type ==
                                    OperationType.Income)
                                .Sum(o =>
                                    o.Amount),
                            group
                                .Where(o =>
                                    o.Type ==
                                    OperationType.Expense)
                                .Sum(o =>
                                    o.Amount)))
                    .ToList();
        }
        else
        {
            timeline =
                operations
                    .GroupBy(o => o.Date)
                    .OrderBy(
                        group =>
                            group.Key)
                    .Select(group =>
                        new TimelineStatisticsDto(
                            group.Key.ToString(
                                "yyyy-MM-dd"),
                            group
                                .Where(o =>
                                    o.Type ==
                                    OperationType.Income)
                                .Sum(o =>
                                    o.Amount),
                            group
                                .Where(o =>
                                    o.Type ==
                                    OperationType.Expense)
                                .Sum(o =>
                                    o.Amount)))
                    .ToList();
        }

        return new StatisticsDto
        {
            From = from,
            To = to,
            Currency = resultCurrency,
            AccountId = resultAccountId,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Net = net,
            ByCategory = byCategory,
            Timeline = timeline
        };
    }

    private static void ValidateParameters(
        StatisticsQueryParameters parameters)
    {
        if (!parameters.From.HasValue)
        {
            throw new BadRequestException(
                "From date is required.");
        }

        if (!parameters.To.HasValue)
        {
            throw new BadRequestException(
                "To date is required.");
        }

        var from =
            parameters.From.Value;

        var to =
            parameters.To.Value;

        if (from > to)
        {
            throw new BadRequestException(
                "From date cannot be later than To date.");
        }

        // Inclusive period
        var numberOfDays =
            to.DayNumber -
            from.DayNumber + 1;

        // R16
        if (numberOfDays > 366)
        {
            throw new BadRequestException(
                "Statistics period cannot exceed 366 days.");
        }

        var hasAccount =
            parameters.AccountId.HasValue;

        var hasCurrency =
            !string.IsNullOrWhiteSpace(
                parameters.Currency);

        // R16:
        // exactly one of AccountId or Currency
        if (hasAccount == hasCurrency)
        {
            throw new BadRequestException(
                "Specify exactly one of accountId or currency.");
        }

        var groupBy =
            string.IsNullOrWhiteSpace(
                parameters.GroupBy)
                ? "day"
                : parameters.GroupBy
                    .Trim()
                    .ToLowerInvariant();

        if (groupBy != "day" &&
            groupBy != "month")
        {
            throw new BadRequestException(
                "GroupBy must be day or month.");
        }
    }

    private void ValidateCurrency(
        string currency)
    {
        if (currency.Length != 3 ||
            !currency.All(char.IsLetter))
        {
            throw new BadRequestException(
                "Currency must be a three-letter ISO code.");
        }

        var configuredCurrencies =
            _configuration
                .GetSection(
                    "Currencies:Allowed")
                .GetChildren()
                .Select(x => x.Value)
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    x!.ToUpperInvariant())
                .ToList();

        if (configuredCurrencies.Count == 0)
        {
            configuredCurrencies =
                new List<string>
                {
                    "RUB",
                    "USD",
                    "EUR",
                    "CNY",
                    "KZT",
                    "GBP"
                };
        }

        if (!configuredCurrencies.Contains(
                currency))
        {
            throw new BadRequestException(
                "Currency is not supported.");
        }
    }

    private static StatisticsDto CreateEmptyResult(
        DateOnly from,
        DateOnly to,
        string? currency,
        Guid? accountId)
    {
        return new StatisticsDto
        {
            From = from,
            To = to,
            Currency = currency,
            AccountId = accountId,
            TotalIncome = 0m,
            TotalExpense = 0m,
            Net = 0m,
            ByCategory =
                Enumerable.Empty<
                    CategoryStatisticsDto>(),
            Timeline =
                Enumerable.Empty<
                    TimelineStatisticsDto>()
        };
    }
}