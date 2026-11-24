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

    public DbSet<ExchangeRate> ExchangeRates =>
        Set<ExchangeRate>();

    public DbSet<RecurringOperation> RecurringOperations =>
        Set<RecurringOperation>();

    public DbSet<Attachment> Attachments =>
        Set<Attachment>();

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

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.UserId);

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

            // FK: Operation -> RecurringOperation
            entity.HasOne<RecurringOperation>()
                .WithMany()
                .HasForeignKey(o => o.RecurringOperationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(o => o.AccountId);

            entity.HasIndex(o => o.CategoryId);

            entity.HasIndex(o => o.RecurringOperationId);

            // One occurrence can create at most one operation.
            entity.HasIndex(o => new
            {
                o.RecurringOperationId,
                o.Date
            })
                .IsUnique();
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

            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.FromAccountId);

            entity.HasIndex(t => t.ToAccountId);
        });

        // =====================================================
        // RECURRING OPERATIONS
        // =====================================================

        modelBuilder.Entity<RecurringOperation>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Amount)
                .HasPrecision(18, 2);

            entity.Property(r => r.Comment)
                .HasMaxLength(500);

            entity.HasOne<Account>()
                .WithMany()
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.AccountId);

            entity.HasIndex(r => r.CategoryId);
        });

        // =====================================================
        // EXCHANGE RATES
        // =====================================================

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(r => new
            {
                r.Date,
                r.Currency
            });

            entity.Property(r => r.Currency)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(r => r.Rate)
                .HasPrecision(18, 6);

            entity.HasIndex(r => new
            {
                r.Date,
                r.Currency
            })
                .IsUnique();
        });

        // =====================================================
        // ATTACHMENTS
        // =====================================================

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.FileName)
                .IsRequired();

            entity.Property(a => a.ContentType)
                .IsRequired();

            entity.Property(a => a.StorageKey)
                .IsRequired();

            entity.HasOne<Operation>()
                .WithMany()
                .HasForeignKey(a => a.OperationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => a.OperationId);
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