using Microsoft.Extensions.Configuration;
using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Services;
using PersonalFinanceTracker.DataAccess.Entities;
using PersonalFinanceTracker.Tests.Helpers;

namespace PersonalFinanceTracker.Tests.Services;

public class AccountServiceTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static IConfiguration CreateConfiguration()
    {
        var settings =
            new Dictionary<string, string?>
            {
                ["Currencies:Allowed:0"] = "RUB",
                ["Currencies:Allowed:1"] = "USD",
                ["Currencies:Allowed:2"] = "EUR",
                ["Currencies:Allowed:3"] = "CNY",
                ["Currencies:Allowed:4"] = "KZT",
                ["Currencies:Allowed:5"] = "GBP"
            };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public async Task R1_Balance_ShouldIncludeOperationsAndTransfers()
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

        var secondAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Second Account",
            Currency = "RUB",
            StartAmount = 0m,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.AddRange(
            account,
            secondAccount);

        db.Operations.AddRange(
            new Operation
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CategoryId = Guid.NewGuid(),
                Type = OperationType.Income,
                Amount = 500m,
                Date = DateOnly.FromDateTime(
                    DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow
            },
            new Operation
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CategoryId = Guid.NewGuid(),
                Type = OperationType.Expense,
                Amount = 200m,
                Date = DateOnly.FromDateTime(
                    DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow
            });

        db.Transfers.AddRange(
            new Transfer
            {
                Id = Guid.NewGuid(),
                FromAccountId = secondAccount.Id,
                ToAccountId = account.Id,
                Amount = 300m,
                Date = DateOnly.FromDateTime(
                    DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow
            },
            new Transfer
            {
                Id = Guid.NewGuid(),
                FromAccountId = account.Id,
                ToAccountId = secondAccount.Id,
                Amount = 100m,
                Date = DateOnly.FromDateTime(
                    DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        var service =
            new AccountService(
                db,
                CreateConfiguration());

        var result =
            await service.GetByIdAsync(
                UserId,
                account.Id);

        Assert.NotNull(result);

        // 1000 + 500 - 200 + 300 - 100 = 1500
        Assert.Equal(
            1500m,
            result.Balance);
    }

    [Fact]
    public async Task R6_DeleteAccountWithOperation_ShouldThrowConflict()
    {
        await using var db =
            TestDbContextFactory.Create();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Used Account",
            Currency = "RUB",
            StartAmount = 1000m,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.Add(account);

        db.Operations.Add(
            new Operation
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CategoryId = Guid.NewGuid(),
                Type = OperationType.Expense,
                Amount = 100m,
                Date = DateOnly.FromDateTime(
                    DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        var service =
            new AccountService(
                db,
                CreateConfiguration());

        await Assert.ThrowsAsync<ConflictException>(
            () => service.DeleteAsync(
                UserId,
                account.Id));
    }

    [Fact]
    public async Task R7_ChangeCurrencyOfUsedAccount_ShouldThrowConflict()
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

        db.Accounts.Add(account);

        db.Operations.Add(
            new Operation
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CategoryId = Guid.NewGuid(),
                Type = OperationType.Expense,
                Amount = 50m,
                Date = DateOnly.FromDateTime(
                    DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        var service =
            new AccountService(
                db,
                CreateConfiguration());

        var request =
            new UpdateAccountRequest
            {
                Name = "Debit Card",
                Currency = "USD",
                StartAmount = 1000m
            };

        await Assert.ThrowsAsync<ConflictException>(
            () => service.UpdateAsync(
                UserId,
                account.Id,
                request));
    }
}