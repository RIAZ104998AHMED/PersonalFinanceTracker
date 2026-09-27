using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class AccountService : IAccountService
{
    private static readonly Guid DemoUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly FinanceTrackerDbContext _dbContext;

    public AccountService(FinanceTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(
        bool includeArchived = false)
    {
        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Where(a =>
                a.UserId == DemoUserId &&
                (includeArchived || !a.IsArchived))
            .ToListAsync();

        var result = new List<AccountDto>();

        foreach (var account in accounts)
        {
            var balance = await CalculateBalanceAsync(account);

            result.Add(MapToDto(account, balance));
        }

        return result;
    }

    public async Task<AccountDto> CreateAsync(
        CreateAccountRequest request)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = DemoUserId,
            Name = request.Name.Trim(),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            StartAmount = request.StartAmount,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        return MapToDto(account, account.StartAmount);
    }

    private async Task<decimal> CalculateBalanceAsync(Account account)
    {
        var income =
            await _dbContext.Operations
                .Where(o =>
                    o.AccountId == account.Id &&
                    o.Type == OperationType.Income)
                .SumAsync(o => (decimal?)o.Amount)
            ?? 0m;

        var expense =
            await _dbContext.Operations
                .Where(o =>
                    o.AccountId == account.Id &&
                    o.Type == OperationType.Expense)
                .SumAsync(o => (decimal?)o.Amount)
            ?? 0m;

        var incomingTransfers =
            await _dbContext.Transfers
                .Where(t => t.ToAccountId == account.Id)
                .SumAsync(t => (decimal?)t.Amount)
            ?? 0m;

        var outgoingTransfers =
            await _dbContext.Transfers
                .Where(t => t.FromAccountId == account.Id)
                .SumAsync(t => (decimal?)t.Amount)
            ?? 0m;

        return account.StartAmount
               + income
               - expense
               + incomingTransfers
               - outgoingTransfers;
    }

    private static AccountDto MapToDto(
        Account account,
        decimal balance)
    {
        return new AccountDto(
            account.Id,
            account.Name,
            account.Currency,
            account.StartAmount,
            account.IsArchived,
            account.CreatedAt,
            balance
        );
    }
}