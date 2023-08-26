using aTES.Common.Shared.Db;
using aTES.TaskTracker.Domain;
using Microsoft.EntityFrameworkCore;
using Task = aTES.TaskTracker.Domain.Task;

namespace aTES.TaskTracker.Db;

public class TaskTrackerDbContext : OutboxContext<TaskTrackerDbContext>
{
    public DbSet<Task> Tasks { get; set; }
    public DbSet<User> Users { get; set; }

    public TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<TaskState>();

        modelBuilder.Entity<Task>(x =>
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

        base.OnModelCreating(modelBuilder);
    }
}