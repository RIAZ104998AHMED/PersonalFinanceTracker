using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class TransferService : ITransferService
{
    private readonly FinanceTrackerDbContext _dbContext;

    public TransferService(FinanceTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TransferDto>> GetAllAsync(
        Guid userId,
        TransferQueryParameters parameters)
    {
        ValidatePaging(parameters);

        var query = _dbContext.Transfers
            .AsNoTracking()
            .Where(t =>
                _dbContext.Accounts.Any(a =>
                    a.Id == t.FromAccountId &&
                    a.UserId == userId));

        var totalCount = await query.CountAsync();

        var transfers = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)parameters.PageSize);

        return new PagedResult<TransferDto>
        {
            Items = transfers.Select(Map).ToList(),
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<TransferDto> GetByIdAsync(
        Guid userId,
        Guid id)
    {
        var transfer = await _dbContext.Transfers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer is null)
        {
            throw new NotFoundException(
                "Transfer was not found.");
        }

        await EnsureTransferOwnershipAsync(
            userId,
            transfer);

        return Map(transfer);
    }

    public async Task<TransferDto> CreateAsync(
        Guid userId,
        CreateTransferRequest request)
    {
        ValidateTransferRequest(request);

        // R8 - transfer cannot use the same account.
        if (request.FromAccountId == request.ToAccountId)
        {
            throw new BadRequestException(
                "From account and To account must be different.");
        }

        var fromAccount =
            await GetAccountAsync(
                userId,
                request.FromAccountId);

        var toAccount =
            await GetAccountAsync(
                userId,
                request.ToAccountId);

        // R5 - both accounts must be active.
        if (fromAccount.IsArchived ||
            toAccount.IsArchived)
        {
            throw new ConflictException(
                "Transfers cannot be created using archived accounts.");
        }

        // R8 - currencies must match.
        if (!string.Equals(
                fromAccount.Currency,
                toAccount.Currency,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                "Transfer accounts must have the same currency.");
        }

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            Date = request.Date,
            Comment = NormalizeComment(request.Comment),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Transfers.Add(transfer);

        await _dbContext.SaveChangesAsync();

        return Map(transfer);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid id)
    {
        var transfer = await _dbContext.Transfers
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transfer is null)
        {
            throw new NotFoundException(
                "Transfer was not found.");
        }

        // R15
        await EnsureTransferOwnershipAsync(
            userId,
            transfer);

        _dbContext.Transfers.Remove(transfer);

        await _dbContext.SaveChangesAsync();
    }

    private async Task<Account> GetAccountAsync(
        Guid userId,
        Guid accountId)
    {
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(
                a => a.Id == accountId);

        if (account is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        // R15 / R8
        if (account.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this account.");
        }

        return account;
    }

    private async Task EnsureTransferOwnershipAsync(
        Guid userId,
        Transfer transfer)
    {
        var fromAccount = await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == transfer.FromAccountId);

        if (fromAccount is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        if (fromAccount.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this transfer.");
        }

        var toAccount = await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == transfer.ToAccountId);

        if (toAccount is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        if (toAccount.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this transfer.");
        }
    }

    private static void ValidateTransferRequest(
        CreateTransferRequest request)
    {
        // R2
        if (request.Amount <= 0)
        {
            throw new BadRequestException(
                "Amount must be greater than zero.");
        }

        // R2
        if (HasMoreThanTwoDecimalPlaces(
                request.Amount))
        {
            throw new BadRequestException(
                "Amount must have at most two fraction digits.");
        }

        // R4
        var todayUtc =
            DateOnly.FromDateTime(DateTime.UtcNow);

        if (request.Date > todayUtc)
        {
            throw new BadRequestException(
                "Transfer date cannot be in the future.");
        }
    }

    private static void ValidatePaging(
        TransferQueryParameters parameters)
    {
        if (parameters.Page < 1)
        {
            throw new BadRequestException(
                "Page must be at least 1.");
        }

        if (parameters.PageSize < 1 ||
            parameters.PageSize > 100)
        {
            throw new BadRequestException(
                "PageSize must be between 1 and 100.");
        }
    }

    private static bool HasMoreThanTwoDecimalPlaces(
        decimal value)
    {
        var bits = decimal.GetBits(value);

        var scale =
            (bits[3] >> 16) & 0xFF;

        return scale > 2;
    }

    private static string? NormalizeComment(
        string? comment)
    {
        return string.IsNullOrWhiteSpace(comment)
            ? null
            : comment.Trim();
    }

    private static TransferDto Map(
        Transfer transfer)
    {
        return new TransferDto(
            transfer.Id,
            transfer.FromAccountId,
            transfer.ToAccountId,
            transfer.Amount,
            transfer.Date,
            transfer.Comment,
            transfer.CreatedAt
        );
    }
}