using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class AccountService : IAccountService
{
    private readonly FinanceTrackerDbContext _dbContext;

    public AccountService(FinanceTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAsync(
        Guid userId,
        bool includeArchived = false)
    {
        var query = _dbContext.Accounts
            .Where(a => a.UserId == userId);

        if (!includeArchived)
        {
            query = query.Where(a => !a.IsArchived);
        }

        var accounts = await query
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        var result = new List<AccountDto>();

        foreach (var account in accounts)
        {
            result.Add(await MapAsync(account));
        }

        return result;
    }

    public async Task<AccountDto?> GetByIdAsync(
        Guid userId,
        Guid id)
    {
        // Find by ID first so we can distinguish
        // 404 from 403.
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);

        // R15: resource does not exist
        if (account is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        // R15: resource belongs to another user
        if (account.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this account.");
        }

        return await MapAsync(account);
    }

    public async Task<AccountDto> CreateAsync(
        Guid userId,
        CreateAccountRequest request)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Currency = request.Currency
                .Trim()
                .ToUpperInvariant(),
            StartAmount = request.StartAmount,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Accounts.Add(account);

        await _dbContext.SaveChangesAsync();

        return await MapAsync(account);
    }

    public async Task<AccountDto?> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateAccountRequest request)
    {
        // Find by ID first.
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);

        // R15: nonexistent account
        if (account is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        // R15: account belongs to another user
        if (account.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this account.");
        }

        var newCurrency = request.Currency
            .Trim()
            .ToUpperInvariant();

        // R7:
        // Currency can only be changed when the
        // account has no operations or transfers.
        if (!string.Equals(
                account.Currency,
                newCurrency,
                StringComparison.OrdinalIgnoreCase))
        {
            var hasOperations =
                await _dbContext.Operations
                    .AnyAsync(o => o.AccountId == id);

            var hasTransfers =
                await _dbContext.Transfers
                    .AnyAsync(t =>
                        t.FromAccountId == id ||
                        t.ToAccountId == id);

            if (hasOperations || hasTransfers)
            {
                throw new ConflictException(
                    "Currency cannot be changed because the account has operations or transfers.");
            }
        }

        account.Name = request.Name.Trim();
        account.Currency = newCurrency;
        account.StartAmount = request.StartAmount;

        await _dbContext.SaveChangesAsync();

        return await MapAsync(account);
    }

    public async Task<AccountDto?> SetArchivedAsync(
        Guid userId,
        Guid id,
        bool isArchived)
    {
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);

        // R15
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

        account.IsArchived = isArchived;

        await _dbContext.SaveChangesAsync();

        return await MapAsync(account);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid id)
    {
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);

        // R15
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

        // R6:
        // Account cannot be deleted if it has
        // operations or transfers.
        var hasOperations =
            await _dbContext.Operations
                .AnyAsync(o => o.AccountId == id);

        var hasTransfers =
            await _dbContext.Transfers
                .AnyAsync(t =>
                    t.FromAccountId == id ||
                    t.ToAccountId == id);

        if (hasOperations || hasTransfers)
        {
            throw new ConflictException(
                "Account cannot be deleted because it has operations or transfers.");
        }

        _dbContext.Accounts.Remove(account);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<AccountDto> MapAsync(Account account)
    {
        var income = await _dbContext.Operations
            .Where(o =>
                o.AccountId == account.Id &&
                o.Type == OperationType.Income)
            .SumAsync(o => (decimal?)o.Amount)
            ?? 0m;

        var expense = await _dbContext.Operations
            .Where(o =>
                o.AccountId == account.Id &&
                o.Type == OperationType.Expense)
            .SumAsync(o => (decimal?)o.Amount)
            ?? 0m;

        var incomingTransfers =
            await _dbContext.Transfers
                .Where(t =>
                    t.ToAccountId == account.Id)
                .SumAsync(t => (decimal?)t.Amount)
            ?? 0m;

        var outgoingTransfers =
            await _dbContext.Transfers
                .Where(t =>
                    t.FromAccountId == account.Id)
                .SumAsync(t => (decimal?)t.Amount)
            ?? 0m;

        var balance =
            account.StartAmount
            + income
            - expense
            + incomingTransfers
            - outgoingTransfers;

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