using PersonalFinanceTracker.BusinessLogic.DTOs;
using PersonalFinanceTracker.BusinessLogic.Exceptions;
using PersonalFinanceTracker.BusinessLogic.Services;
using PersonalFinanceTracker.DataAccess.Entities;
using PersonalFinanceTracker.Tests.Helpers;

namespace PersonalFinanceTracker.Tests.Services;

public class TransferServiceTests
{
    private static readonly Guid UserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task R8_SameAccountTransfer_ShouldThrowBadRequest()
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

        db.Accounts.Add(account);

        await db.SaveChangesAsync();

        var service =
            new TransferService(db);

        var request =
            new CreateTransferRequest
            {
                FromAccountId = account.Id,
                ToAccountId = account.Id,
                Amount = 100m,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Comment = "Invalid same-account transfer"
            };

        await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateAsync(
                UserId,
                request));
    }

    [Fact]
    public async Task R8_DifferentCurrencyTransfer_ShouldThrowBadRequest()
    {
        await using var db =
            TestDbContextFactory.Create();

        var rubAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "RUB Account",
            Currency = "RUB",
            StartAmount = 1000m,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var usdAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "USD Account",
            Currency = "USD",
            StartAmount = 1000m,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.AddRange(
            rubAccount,
            usdAccount);

        await db.SaveChangesAsync();

        var service =
            new TransferService(db);

        var request =
            new CreateTransferRequest
            {
                FromAccountId = rubAccount.Id,
                ToAccountId = usdAccount.Id,
                Amount = 100m,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Comment = "Different currency test"
            };

        await Assert.ThrowsAsync<BadRequestException>(
            () => service.CreateAsync(
                UserId,
                request));
    }

    [Fact]
    public async Task R5_ArchivedAccountTransfer_ShouldThrowConflict()
    {
        await using var db =
            TestDbContextFactory.Create();

        var activeAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Active Account",
            Currency = "RUB",
            StartAmount = 1000m,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var archivedAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = "Archived Account",
            Currency = "RUB",
            StartAmount = 1000m,
            IsArchived = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Accounts.AddRange(
            activeAccount,
            archivedAccount);

        await db.SaveChangesAsync();

        var service =
            new TransferService(db);

        var request =
            new CreateTransferRequest
            {
                FromAccountId = activeAccount.Id,
                ToAccountId = archivedAccount.Id,
                Amount = 100m,
                Date = DateOnly.FromDateTime(DateTime.UtcNow)
            };

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                UserId,
                request));
    }
}