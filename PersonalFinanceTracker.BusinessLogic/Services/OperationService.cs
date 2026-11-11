using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic.Services;

public class OperationService : IOperationService
{
    private readonly FinanceTrackerDbContext _dbContext;

    public OperationService(
        FinanceTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<OperationDto>> GetAllAsync(
        Guid userId,
        OperationQueryParameters parameters)
    {
        ValidateQueryParameters(parameters);

        var query = _dbContext.Operations
            .AsNoTracking()
            .Where(o =>
                _dbContext.Accounts.Any(a =>
                    a.Id == o.AccountId &&
                    a.UserId == userId));

        if (parameters.AccountId.HasValue)
        {
            query = query.Where(
                o => o.AccountId ==
                     parameters.AccountId.Value);
        }

        if (parameters.CategoryIds is { Count: > 0 })
        {
            query = query.Where(
                o => parameters.CategoryIds.Contains(o.CategoryId));
        }
        if (parameters.Type.HasValue)
        {
            query = query.Where(
                o => o.Type ==
                     parameters.Type.Value);
        }

        if (parameters.From.HasValue)
        {
            query = query.Where(
                o => o.Date >=
                     parameters.From.Value);
        }

        if (parameters.To.HasValue)
        {
            query = query.Where(
                o => o.Date <=
                     parameters.To.Value);
        }

        var totalCount =
            await query.CountAsync();

        query =
            parameters.SortBy
                .ToLowerInvariant() switch
            {
                "datedesc" =>
                    query
                        .OrderByDescending(
                            o => o.Date)
                        .ThenByDescending(
                            o => o.CreatedAt),

                "dateasc" =>
                    query
                        .OrderBy(
                            o => o.Date)
                        .ThenByDescending(
                            o => o.CreatedAt),

                "amountdesc" =>
                    query
                        .OrderByDescending(
                            o => o.Amount)
                        .ThenByDescending(
                            o => o.CreatedAt),

                "amountasc" =>
                    query
                        .OrderBy(
                            o => o.Amount)
                        .ThenByDescending(
                            o => o.CreatedAt),

                _ => throw new BadRequestException(
                    "Invalid sortBy value.")
            };

        var operations =
            await query
                .Skip(
                    (parameters.Page - 1) *
                    parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

        // Load all required accounts in one query.
        var accountIds = operations
            .Select(o => o.AccountId)
            .Distinct()
            .ToList();

        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Where(a =>
                accountIds.Contains(a.Id))
            .ToDictionaryAsync(
                a => a.Id);

        // Load all required categories in one query.
        var categoryIds = operations
            .Select(o => o.CategoryId)
            .Distinct()
            .ToList();

        var categories =
            await _dbContext.Categories
                .AsNoTracking()
                .Where(c =>
                    categoryIds.Contains(c.Id))
                .ToDictionaryAsync(
                    c => c.Id);

        var items = new List<OperationDto>();

        foreach (var operation in operations)
        {
            if (!accounts.TryGetValue(
                    operation.AccountId,
                    out var account))
            {
                throw new NotFoundException(
                    "Account was not found.");
            }

            if (!categories.TryGetValue(
                    operation.CategoryId,
                    out var category))
            {
                throw new NotFoundException(
                    "Category was not found.");
            }

            items.Add(
                Map(
                    operation,
                    account.Name,
                    account.Currency,
                    category.Name));
        }

        var totalPages =
            (int)Math.Ceiling(
                totalCount /
                (double)parameters.PageSize);

        return new PagedResult<OperationDto>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<OperationDto> GetByIdAsync(
        Guid userId,
        Guid id)
    {
        var operation =
            await _dbContext.Operations
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    o => o.Id == id);

        if (operation is null)
        {
            throw new NotFoundException(
                "Operation was not found.");
        }

        await EnsureOperationOwnershipAsync(
            userId,
            operation);

        var account =
            await GetAccountForOperationAsync(
                userId,
                operation.AccountId);

        var category =
            await GetCategoryForOperationAsync(
                userId,
                operation.CategoryId);

        return Map(
            operation,
            account.Name,
            account.Currency,
            category.Name);
    }

    public async Task<OperationDto> CreateAsync(
    Guid userId,
    CreateOperationRequest request)
    {
        if (!request.AccountId.HasValue)
        {
            throw new BadRequestException(
                "AccountId is required.");
        }

        if (!request.CategoryId.HasValue)
        {
            throw new BadRequestException(
                "CategoryId is required.");
        }

        ValidateOperationRequest(
            request.Type,
            request.Amount,
            request.Date);

        var account =
            await GetAccountForOperationAsync(
                userId,
                request.AccountId.Value);

        if (account.IsArchived)
        {
            throw new ConflictException(
                "Operation cannot be created on an archived account.");
        }

        var category =
            await GetCategoryForOperationAsync(
                userId,
                request.CategoryId.Value);

        if (category.Type != request.Type!.Value)
        {
            throw new BadRequestException(
                "Category type must match operation type.");
        }

        var operation = new Operation
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId.Value,
            CategoryId = request.CategoryId.Value,
            Type = request.Type.Value,
            Amount = request.Amount,
            Date = request.Date!.Value,
            Comment = NormalizeComment(request.Comment),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Operations.Add(operation);

        await _dbContext.SaveChangesAsync();

        return Map(
            operation,
            account.Name,
            account.Currency,
            category.Name);
    }

    public async Task<OperationDto> UpdateAsync(
    Guid userId,
    Guid id,
    UpdateOperationRequest request)
    {
        var operation =
            await _dbContext.Operations
                .FirstOrDefaultAsync(
                    o => o.Id == id);

        if (operation is null)
        {
            throw new NotFoundException(
                "Operation was not found.");
        }

        await EnsureOperationOwnershipAsync(
            userId,
            operation);

        if (!request.AccountId.HasValue)
        {
            throw new BadRequestException(
                "AccountId is required.");
        }

        if (!request.CategoryId.HasValue)
        {
            throw new BadRequestException(
                "CategoryId is required.");
        }

        ValidateOperationRequest(
            request.Type,
            request.Amount,
            request.Date);

        var targetAccount =
            await GetAccountForOperationAsync(
                userId,
                request.AccountId.Value);

        if (request.AccountId.Value !=
                operation.AccountId &&
            targetAccount.IsArchived)
        {
            throw new ConflictException(
                "Operation cannot be moved to an archived account.");
        }

        var category =
            await GetCategoryForOperationAsync(
                userId,
                request.CategoryId.Value);

        if (category.Type != request.Type!.Value)
        {
            throw new BadRequestException(
                "Category type must match operation type.");
        }

        operation.AccountId =
            request.AccountId.Value;

        operation.CategoryId =
            request.CategoryId.Value;

        operation.Type =
            request.Type.Value;

        operation.Amount =
            request.Amount;

        operation.Date =
            request.Date!.Value;

        operation.Comment =
            NormalizeComment(request.Comment);

        await _dbContext.SaveChangesAsync();

        return Map(
            operation,
            targetAccount.Name,
            targetAccount.Currency,
            category.Name);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid id)
    {
        var operation =
            await _dbContext.Operations
                .FirstOrDefaultAsync(
                    o => o.Id == id);

        if (operation is null)
        {
            throw new NotFoundException(
                "Operation was not found.");
        }

        // R15
        await EnsureOperationOwnershipAsync(
            userId,
            operation);

        _dbContext.Operations.Remove(
            operation);

        await _dbContext.SaveChangesAsync();
    }

    private async Task<Account>
        GetAccountForOperationAsync(
            Guid userId,
            Guid accountId)
    {
        var account =
            await _dbContext.Accounts
                .FirstOrDefaultAsync(
                    a => a.Id == accountId);

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

        return account;
    }

    private async Task<Category>
        GetCategoryForOperationAsync(
            Guid userId,
            Guid categoryId)
    {
        var category =
            await _dbContext.Categories
                .FirstOrDefaultAsync(
                    c => c.Id ==
                         categoryId);

        if (category is null)
        {
            throw new NotFoundException(
                "Category was not found.");
        }

        // System category is allowed.
        // User category is allowed only
        // for its owner.
        if (category.UserId.HasValue &&
            category.UserId.Value !=
            userId)
        {
            throw new ForbiddenException(
                "You do not have access to this category.");
        }

        return category;
    }

    private async Task
        EnsureOperationOwnershipAsync(
            Guid userId,
            Operation operation)
    {
        var account =
            await _dbContext.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    a => a.Id ==
                         operation.AccountId);

        if (account is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        // R15
        if (account.UserId != userId)
        {
            throw new ForbiddenException(
                "You do not have access to this operation.");
        }
    }

    private static void ValidateOperationRequest(
    OperationType? type,
    decimal amount,
    DateOnly? date)
    {
        if (!type.HasValue)
        {
            throw new BadRequestException(
                "Operation type is required.");
        }

        if (!date.HasValue)
        {
            throw new BadRequestException(
                "Operation date is required.");
        }

        if (amount <= 0)
        {
            throw new BadRequestException(
                "Amount must be greater than zero.");
        }

        if (HasMoreThanTwoDecimalPlaces(amount))
        {
            throw new BadRequestException(
                "Amount must have at most two fraction digits.");
        }

        var todayUtc =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        if (date.Value > todayUtc)
        {
            throw new BadRequestException(
                "Operation date cannot be in the future.");
        }
    }
    private static void ValidateQueryParameters(
    OperationQueryParameters parameters)
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

        if (parameters.From.HasValue &&
            parameters.To.HasValue &&
            parameters.From.Value >
            parameters.To.Value)
        {
            throw new BadRequestException(
                "From date cannot be later than To date.");
        }

        var allowedSortValues = new[]
        {
        "datedesc",
        "dateasc",
        "amountdesc",
        "amountasc"
    };

        if (!allowedSortValues.Contains(
                parameters.SortBy.ToLowerInvariant()))
        {
            throw new BadRequestException(
                "SortBy must be dateDesc, dateAsc, amountDesc or amountAsc.");
        }
    }

    private static bool
        HasMoreThanTwoDecimalPlaces(
            decimal value)

    {
        var bits =
            decimal.GetBits(value);

        var scale =
            (bits[3] >> 16) & 0xFF;

        return scale > 2;
    }

    private static string?
        NormalizeComment(
            string? comment)
    {
        return string.IsNullOrWhiteSpace(
                comment)
            ? null
            : comment.Trim();
    }

    private static OperationDto Map(
        Operation operation,
        string accountName,
        string currency,
        string categoryName)
    {
        return new OperationDto(
            operation.Id,
            operation.AccountId,
            accountName,
            currency,
            operation.CategoryId,
            categoryName,
            operation.Type,
            operation.Amount,
            operation.Date,
            operation.Comment,
            operation.CreatedAt
        );
    }
}
