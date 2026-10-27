using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceTracker.DataAccess.Data;
using PersonalFinanceTracker.DataAccess.Entities;

namespace PersonalFinanceTracker.BusinessLogic;

public static class DatabaseStartup
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<FinanceTrackerDbContext>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        // Apply database migrations
        await dbContext.Database.MigrateAsync();

        // Seed demo Identity user
        await SeedDemoUserAsync(userManager);

        // Seed categories and existing application data
        await DatabaseSeeder.SeedAsync(dbContext);
    }

    private static async Task SeedDemoUserAsync(
        UserManager<ApplicationUser> userManager)
    {
        var demoUserId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111111111");

        const string demoEmail =
            "demo@fintracker.local";

        const string demoPassword =
            "Demo12345";

        // Check whether the fixed demo user already exists
        var existingUser =
            await userManager.FindByIdAsync(
                demoUserId.ToString());

        if (existingUser is not null)
        {
            return;
        }

        // Also check the e-mail to avoid duplicates
        existingUser =
            await userManager.FindByEmailAsync(
                demoEmail);

        if (existingUser is not null)
        {
            return;
        }

        var demoUser = new ApplicationUser
        {
            Id = demoUserId,
            UserName = demoEmail,
            Email = demoEmail,
            Name = "Demo User",
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result =
            await userManager.CreateAsync(
                demoUser,
                demoPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(
                    e => e.Description));

            throw new InvalidOperationException(
                $"Failed to seed demo user: {errors}");
        }
    }
}