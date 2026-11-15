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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // ACCOUNTS
        // =====================================================

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.Currency)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(a => a.StartAmount)
                .HasPrecision(18, 2);

            // FK: Account -> User
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index on FK
            entity.HasIndex(a => a.UserId);
        });

        // =====================================================
        // CATEGORIES
        // =====================================================

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

            // System categories have UserId = null.
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index on FK
            entity.HasIndex(c => c.UserId);

            // Required by assignment:
            // unique category per UserId + Name + Type
            entity.HasIndex(c => new
            {
                c.UserId,
                c.Name,
                c.Type
            })
                .IsUnique();
        });

        // =====================================================
        // OPERATIONS
        // =====================================================

        modelBuilder.Entity<Operation>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Amount)
                .HasPrecision(18, 2);

            entity.Property(o => o.Comment)
                .HasMaxLength(500);

            // FK: Operation -> Account
            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(o => o.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Operation -> Category
            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(o => o.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes on foreign keys
            entity.HasIndex(o => o.AccountId);
            entity.HasIndex(o => o.CategoryId);
        });

        // =====================================================
        // TRANSFERS
        // =====================================================

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            entity.Property(t => t.Comment)
                .HasMaxLength(500);

            // FK: Transfer -> FromAccount
            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Transfer -> ToAccount
            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes on foreign keys
            entity.HasIndex(t => t.FromAccountId);
            entity.HasIndex(t => t.ToAccountId);
        });

        // =====================================================
        // REFRESH TOKENS
        // =====================================================

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