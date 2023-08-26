using aTES.Auth.Models;
using aTES.Common.Shared.Db;
using Microsoft.EntityFrameworkCore;

namespace aTES.Auth.Data;

public class ApplicationDbContext : OutboxContext<ApplicationDbContext>
{
    public DbSet<PopugUser> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<PopugRoles>();

        modelBuilder.Entity<PopugUser>(x =>
        {
            x.HasKey(c => c.Id);
            x.Property(c => c.Id).ValueGeneratedOnAdd();
            x.HasIndex(c => c.Email).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}