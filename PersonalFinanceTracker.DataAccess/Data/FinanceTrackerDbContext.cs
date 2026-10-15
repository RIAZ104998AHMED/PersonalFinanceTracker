using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace PersonalFinanceTracker.DataAccess.Data;

public class FinanceTrackerDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public FinanceTrackerDbContext(
        DbContextOptions<FinanceTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Operation> Operations => Set<Operation>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .Property(a => a.StartAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Operation>()
            .Property(o => o.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transfer>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Account>()
            .Property(a => a.Currency)
            .HasMaxLength(3);
    }
}
