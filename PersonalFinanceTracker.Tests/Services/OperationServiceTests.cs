using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Services;
using PersonalFinanceTracker.DataAccess.Entities;
using PersonalFinanceTracker.Tests.Helpers;

namespace PersonalFinanceTracker.Tests.Services;

public class OperationServiceTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task R3_CategoryTypeMismatch_ShouldThrowBadRequest()
    {
        await using var db =
            TestDbContextFactory.Create();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Debit Card",
            Currency = "RUB",
            StartAmount = 1000m,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var incomeCategory = new Category
        {
            Id = Guid.NewGuid(),
            UserId = null,
            Name = "Salary",
            Type = OperationType.Income
        };

        db.Accounts.Add(account);
        db.Categories.Add(incomeCategory);

        await db.SaveChangesAsync();

        var service =
            new OperationService(db);

        var request =
            new CreateOperationRequest
            {
                AccountId = account.Id,
                CategoryId = incomeCategory.Id,

                // Intentionally wrong:
                // category = Income,
                // operation = Expense
                Type = OperationType.Expense,

                Amount = 100m,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Comment = "Wrong category type"
            };

        await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateAsync(
                UserId,
                request));
    }

    [Fact]
    public async Task R5_CreateOperationOnArchivedAccount_ShouldThrowConflict()
    {
        await using var db =
            TestDbContextFactory.Create();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Archived Account",
            Currency = "RUB",
            StartAmount = 1000m,
            IsArchived = true,
            CreatedAt = DateTime.UtcNow
        };

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = null,
            Name = "Food",
            Type = OperationType.Expense
        };

        db.Accounts.Add(account);
        db.Categories.Add(category);

        await db.SaveChangesAsync();

        var service =
            new OperationService(db);

        var request =
            new CreateOperationRequest
            {
                AccountId = account.Id,
                CategoryId = category.Id,
                Type = OperationType.Expense,
                Amount = 100m,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Comment = "Archived account test"
            };

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                UserId,
                request));
    }

    [Fact]
    public async Task R4_FutureDate_ShouldThrowBadRequest()
    {
        await using var db =
            TestDbContextFactory.Create();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Main Account",
            Currency = "RUB",
            StartAmount = 1000m,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = null,
            Name = "Food",
            Type = OperationType.Expense
        };

        db.Accounts.Add(account);
        db.Categories.Add(category);

        await db.SaveChangesAsync();

        var service =
            new OperationService(db);

        var request =
            new CreateOperationRequest
            {
                AccountId = account.Id,
                CategoryId = category.Id,
                Type = OperationType.Expense,
                Amount = 100m,

                Date =
                    DateOnly.FromDateTime(
                        DateTime.UtcNow.AddDays(1)),

                Comment = "Future operation"
            };

        await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateAsync(
                UserId,
                request));
    }
}