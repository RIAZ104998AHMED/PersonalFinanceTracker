using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.DataAccess.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(FinanceTrackerDbContext dbContext)
    {
      
        if (await dbContext.Categories.AnyAsync(c => c.UserId == null))
        {
            return;
        }

        var categories = new List<Category>
        {
            // Income
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

            // Expense
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

        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.SaveChangesAsync();
    }
}