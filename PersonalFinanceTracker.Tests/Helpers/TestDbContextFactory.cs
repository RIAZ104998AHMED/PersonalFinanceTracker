using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.DataAccess.Data;

namespace PersonalFinanceTracker.Tests.Helpers;

public static class TestDbContextFactory
{
    public static FinanceTrackerDbContext Create()
    {
        var options =
            new DbContextOptionsBuilder<FinanceTrackerDbContext>()
                .UseInMemoryDatabase(
                    $"FinanceTrackerTests_{Guid.NewGuid()}")
                .Options;

        return new FinanceTrackerDbContext(options);
    }
}