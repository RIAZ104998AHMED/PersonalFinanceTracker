using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.DataAccess.Entities;

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

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Accounts
        modelBuilder.Entity<Account>()
            .Property(a => a.StartAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Account>()
            .Property(a => a.Currency)
            .HasMaxLength(3);

        // Operations
        modelBuilder.Entity<Operation>()
            .Property(o => o.Amount)
            .HasPrecision(18, 2);

        // Transfers
        modelBuilder.Entity<Transfer>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        // Refresh tokens
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(x => x.TokenHash)
                .IsUnique();

            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}