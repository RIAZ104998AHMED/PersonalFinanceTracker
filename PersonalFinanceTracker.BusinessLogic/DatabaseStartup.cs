using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.DataAccess.Data;

namespace PersonalFinanceTracker.BusinessLogic;

public static class DatabaseStartup
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<FinanceTrackerDbContext>();

        await dbContext.Database.MigrateAsync();

        await DatabaseSeeder.SeedAsync(dbContext);
    }
}