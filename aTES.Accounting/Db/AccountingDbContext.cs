using System.Runtime.Intrinsics.X86;
using aTES.Accounting.Domain;
using Microsoft.EntityFrameworkCore;

namespace aTES.Accounting.Db;

public class AccountingDbContext : DbContext
{
    public DbSet<StreamedTask> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<PopugTransaction> Transactions  { get; set; }
    public DbSet<AuditLogItem> AuditLogItems { get; set; }
    public DbSet<BillingCycle> BillingCycles { get; set; }

    public AccountingDbContext(DbContextOptions<AccountingDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StreamedTask>(x =>
        {
            x.HasKey(c => c.Id);
            x.Property(c => c.Id).ValueGeneratedOnAdd();
            x.HasIndex(c => c.PublicId).IsUnique();
        });

        modelBuilder.Entity<User>(x =>
        {
            x.HasKey(c => c.Id);
            x.Property(c => c.Id).ValueGeneratedOnAdd();
            x.HasIndex(c => c.PublicId).IsUnique();
        });

        // modelBuilder.Entity<AuditLogItem>(x =>
        // {
        //     x.HasKey(c => c.Id);
        //     x.Property(c => c.Id).ValueGeneratedOnAdd();
        // });

        modelBuilder.Entity<Account>(x =>
        {
            x.HasKey(c => c.Id);
            x.Property(c => c.Id).ValueGeneratedOnAdd();
                // x.HasMany(c => c.AuditLog)
                // .WithOne(c => c.OwnerAccount)
                // .HasForeignKey(c => c.AccountId);
            x.HasMany(c => c.Transactions).WithOne().HasForeignKey(c => c.AccountId);
        });

        modelBuilder.Entity<PopugTransaction>(x =>
        {
            x.HasKey(c => c.Id);
            x.Property(c => c.Id).ValueGeneratedOnAdd();
            x.HasOne(c => c.BillingCycle)
                .WithMany(c => c.Transactions)
                .HasForeignKey(c => c.BillingCycleId);
        });

        modelBuilder.Entity<BillingCycle>(x =>
        {
            x.HasKey(c => c.Id);
            x.Property(c => c.Id).ValueGeneratedOnAdd();
        });

        base.OnModelCreating(modelBuilder);
    }
}