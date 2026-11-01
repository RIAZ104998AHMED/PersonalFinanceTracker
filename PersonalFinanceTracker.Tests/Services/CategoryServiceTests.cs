using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Services;
using PersonalFinanceTracker.DataAccess.Entities;
using PersonalFinanceTracker.Tests.Helpers;

namespace PersonalFinanceTracker.Tests.Services;

public class CategoryServiceTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task R10_DuplicateCategory_ShouldThrowConflict()
    {
        await using var db =
            TestDbContextFactory.Create();

        var existingCategory = new Category
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Travel",
            Type = OperationType.Expense
        };

        db.Categories.Add(existingCategory);

        await db.SaveChangesAsync();

        var service =
            new CategoryService(db);

        var request =
            new CreateCategoryRequest
            {
                Name = "Travel",
                Type = OperationType.Expense
            };

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                UserId,
                request));
    }

    [Fact]
    public async Task R10_DuplicateSystemCategory_ShouldThrowConflict()
    {
        await using var db =
            TestDbContextFactory.Create();

        var systemCategory = new Category
        {
            Id = Guid.NewGuid(),
            UserId = null,
            Name = "Food",
            Type = OperationType.Expense
        };

        db.Categories.Add(systemCategory);

        await db.SaveChangesAsync();

        var service =
            new CategoryService(db);

        var request =
            new CreateCategoryRequest
            {
                Name = "food",
                Type = OperationType.Expense
            };

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                UserId,
                request));
    }

    [Fact]
    public async Task R12_UsedCategory_ShouldNotBeDeleted()
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
            UserId = UserId,
            Name = "Travel",
            Type = OperationType.Expense
        };

        var operation = new Operation
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = OperationType.Expense,
            Amount = 100m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.Add(account);
        db.Categories.Add(category);
        db.Operations.Add(operation);

        await db.SaveChangesAsync();

        var service =
            new CategoryService(db);

        await Assert.ThrowsAsync<ConflictException>(
            () => service.DeleteAsync(
                UserId,
                category.Id));
    }
}