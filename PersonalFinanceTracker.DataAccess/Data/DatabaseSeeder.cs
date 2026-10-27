using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.DataAccess.Data;

public static class DatabaseSeeder
{
    private static readonly Guid DemoUserId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid CashId =
        Guid.Parse("c0000000-0000-0000-0000-000000000001");

    private static readonly Guid DebitCardId =
        Guid.Parse("c0000000-0000-0000-0000-000000000002");

    private static readonly Guid SavingsId =
        Guid.Parse("c0000000-0000-0000-0000-000000000003");

    public static async Task SeedAsync(
        FinanceTrackerDbContext dbContext)
    {
        var seedTime = DateTime.UtcNow;

        await SeedSystemCategoriesAsync(dbContext);
        await SeedDemoAccountsAsync(dbContext, seedTime);
        await SeedDemoOperationsAsync(dbContext, seedTime);
        await SeedDemoTransfersAsync(dbContext, seedTime);
    }

    private static async Task SeedSystemCategoriesAsync(
        FinanceTrackerDbContext dbContext)
    {
        var categories = new List<Category>
        {
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                UserId = null,
                Name = "Salary",
                Type = OperationType.Income
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                UserId = null,
                Name = "Scholarship",
                Type = OperationType.Income
            },
            new()
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                UserId = null,
                Name = "Other",
                Type = OperationType.Income
            },

            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000001"),
                UserId = null,
                Name = "Food",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000002"),
                UserId = null,
                Name = "Restaurants",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000003"),
                UserId = null,
                Name = "Medicine",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000004"),
                UserId = null,
                Name = "Sport",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000005"),
                UserId = null,
                Name = "Taxi",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000006"),
                UserId = null,
                Name = "Rent",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000007"),
                UserId = null,
                Name = "Investments",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000008"),
                UserId = null,
                Name = "Clothes",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000009"),
                UserId = null,
                Name = "Fun",
                Type = OperationType.Expense
            },
            new()
            {
                Id = Guid.Parse("b0000000-0000-0000-0000-000000000010"),
                UserId = null,
                Name = "Other",
                Type = OperationType.Expense
            }
        };

        foreach (var category in categories)
        {
            if (!await dbContext.Categories
                    .AnyAsync(c => c.Id == category.Id))
            {
                await dbContext.Categories.AddAsync(category);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDemoAccountsAsync(
        FinanceTrackerDbContext dbContext,
        DateTime seedTime)
    {
        var accounts = new[]
        {
            new Account
            {
                Id = CashId,
                UserId = DemoUserId,
                Name = "Cash",
                Currency = "RUB",
                StartAmount = 5000.00m,
                IsArchived = false,
                CreatedAt = seedTime
            },

            new Account
            {
                Id = DebitCardId,
                UserId = DemoUserId,
                Name = "Debit card",
                Currency = "RUB",
                StartAmount = 42000.00m,
                IsArchived = false,
                CreatedAt = seedTime
            },

            new Account
            {
                Id = SavingsId,
                UserId = DemoUserId,
                Name = "Savings",
                Currency = "USD",
                StartAmount = 1500.00m,
                IsArchived = false,
                CreatedAt = seedTime
            }
        };

        foreach (var account in accounts)
        {
            if (!await dbContext.Accounts
                    .AnyAsync(a => a.Id == account.Id))
            {
                await dbContext.Accounts.AddAsync(account);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDemoOperationsAsync(
        FinanceTrackerDbContext dbContext,
        DateTime seedTime)
    {
        var salary =
            Guid.Parse("a0000000-0000-0000-0000-000000000001");

        var scholarship =
            Guid.Parse("a0000000-0000-0000-0000-000000000002");

        var otherIncome =
            Guid.Parse("a0000000-0000-0000-0000-000000000003");

        var food =
            Guid.Parse("b0000000-0000-0000-0000-000000000001");

        var restaurants =
            Guid.Parse("b0000000-0000-0000-0000-000000000002");

        var medicine =
            Guid.Parse("b0000000-0000-0000-0000-000000000003");

        var sport =
            Guid.Parse("b0000000-0000-0000-0000-000000000004");

        var taxi =
            Guid.Parse("b0000000-0000-0000-0000-000000000005");

        var rent =
            Guid.Parse("b0000000-0000-0000-0000-000000000006");

        var investments =
            Guid.Parse("b0000000-0000-0000-0000-000000000007");

        var clothes =
            Guid.Parse("b0000000-0000-0000-0000-000000000008");

        var fun =
            Guid.Parse("b0000000-0000-0000-0000-000000000009");

        var otherExpense =
            Guid.Parse("b0000000-0000-0000-0000-000000000010");

        var operations = new[]
        {
            Op(1, DebitCardId, salary, OperationType.Income,
                60000m, 2026, 8, 5, "August salary", seedTime),

            Op(2, DebitCardId, rent, OperationType.Expense,
                25000m, 2026, 8, 5, "Rent, August", seedTime),

            Op(3, DebitCardId, food, OperationType.Expense,
                3200.50m, 2026, 8, 6, "Groceries", seedTime),

            Op(4, DebitCardId, restaurants, OperationType.Expense,
                1800m, 2026, 8, 9, "Dinner", seedTime),

            Op(5, DebitCardId, food, OperationType.Expense,
                2750m, 2026, 8, 12, "Groceries", seedTime),

            Op(6, DebitCardId, clothes, OperationType.Expense,
                4990m, 2026, 8, 14, "Sneakers", seedTime),

            Op(7, DebitCardId, food, OperationType.Expense,
                3100.25m, 2026, 8, 18, "Groceries", seedTime),

            Op(8, DebitCardId, medicine, OperationType.Expense,
                1200m, 2026, 8, 20, "Pharmacy", seedTime),

            Op(9, DebitCardId, investments, OperationType.Expense,
                10000m, 2026, 8, 25, "Index fund", seedTime),

            Op(10, DebitCardId, salary, OperationType.Income,
                60000m, 2026, 9, 5, "September salary", seedTime),

            Op(11, DebitCardId, rent, OperationType.Expense,
                25000m, 2026, 9, 5, "Rent, September", seedTime),

            Op(12, DebitCardId, food, OperationType.Expense,
                2980m, 2026, 9, 7, "Groceries", seedTime),

            Op(13, DebitCardId, sport, OperationType.Expense,
                3500m, 2026, 9, 10, "Gym membership", seedTime),

            Op(14, DebitCardId, restaurants, OperationType.Expense,
                2400m, 2026, 9, 13, "Lunch with friends", seedTime),

            Op(15, DebitCardId, food, OperationType.Expense,
                3050.75m, 2026, 9, 17, "Groceries", seedTime),

            Op(16, DebitCardId, fun, OperationType.Expense,
                1500m, 2026, 9, 21, "Cinema", seedTime),

            Op(17, DebitCardId, food, OperationType.Expense,
                2600m, 2026, 9, 26, "Groceries", seedTime),

            Op(18, DebitCardId, investments, OperationType.Expense,
                10000m, 2026, 9, 28, "Index fund", seedTime),

            Op(19, CashId, taxi, OperationType.Expense,
                450m, 2026, 8, 2, null, seedTime),

            Op(20, CashId, food, OperationType.Expense,
                780m, 2026, 8, 10, "Market", seedTime),

            Op(21, CashId, otherIncome, OperationType.Income,
                3000m, 2026, 8, 15, "Birthday gift", seedTime),

            Op(22, CashId, taxi, OperationType.Expense,
                520m, 2026, 8, 22, null, seedTime),

            Op(23, CashId, fun, OperationType.Expense,
                1200m, 2026, 8, 29, "Concert", seedTime),

            Op(24, CashId, taxi, OperationType.Expense,
                610m, 2026, 9, 3, null, seedTime),

            Op(25, CashId, food, OperationType.Expense,
                900m, 2026, 9, 11, "Market", seedTime),

            Op(26, CashId, otherExpense, OperationType.Expense,
                350m, 2026, 9, 19, "Haircut", seedTime),

            Op(27, CashId, taxi, OperationType.Expense,
                480m, 2026, 9, 24, null, seedTime),

            Op(28, SavingsId, scholarship, OperationType.Income,
                300m, 2026, 8, 1, "Scholarship, August", seedTime),

            Op(29, SavingsId, scholarship, OperationType.Income,
                300m, 2026, 9, 1, "Scholarship, September", seedTime),

            Op(30, SavingsId, otherExpense, OperationType.Expense,
                45m, 2026, 9, 15, "Bank fee", seedTime)
        };

        foreach (var operation in operations)
        {
            if (!await dbContext.Operations
                    .AnyAsync(o => o.Id == operation.Id))
            {
                await dbContext.Operations.AddAsync(operation);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDemoTransfersAsync(
        FinanceTrackerDbContext dbContext,
        DateTime seedTime)
    {
        var transfers = new[]
        {
            new Transfer
            {
                Id = Guid.Parse(
                    "e0000000-0000-0000-0000-000000000001"),
                FromAccountId = DebitCardId,
                ToAccountId = CashId,
                Amount = 5000.00m,
                Date = new DateOnly(2026, 8, 7),
                Comment = "Cash withdrawal",
                CreatedAt = seedTime
            },

            new Transfer
            {
                Id = Guid.Parse(
                    "e0000000-0000-0000-0000-000000000002"),
                FromAccountId = DebitCardId,
                ToAccountId = CashId,
                Amount = 5000.00m,
                Date = new DateOnly(2026, 9, 8),
                Comment = "Cash withdrawal",
                CreatedAt = seedTime
            }
        };

        foreach (var transfer in transfers)
        {
            if (!await dbContext.Transfers
                    .AnyAsync(t => t.Id == transfer.Id))
            {
                await dbContext.Transfers.AddAsync(transfer);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static Operation Op(
        int number,
        Guid accountId,
        Guid categoryId,
        OperationType type,
        decimal amount,
        int year,
        int month,
        int day,
        string? comment,
        DateTime createdAt)
    {
        return new Operation
        {
            Id = Guid.Parse(
                $"d0000000-0000-0000-0000-{number:000000000000}"),
            AccountId = accountId,
            CategoryId = categoryId,
            Type = type,
            Amount = amount,
            Date = new DateOnly(year, month, day),
            Comment = comment,
            CreatedAt = createdAt
        };
    }
}