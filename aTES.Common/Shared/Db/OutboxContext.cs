using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace aTES.Common.Shared.Db;

public class OutboxContext<TContext> : DbContext where TContext : DbContext
{
    public OutboxContext(DbContextOptions<TContext> options) : base(options)
    {
    }

    public DbSet<OutboxItem> OutboxItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxItem>(x =>
        {
            x.HasKey(c => c.Id);
            x.Property(c => c.Id).ValueGeneratedOnAdd();
        });

        base.OnModelCreating(modelBuilder);
    }

    public void Produce<TMessageKey, TMessageValue>(string topic, Message<TMessageKey, TMessageValue> message)
    {
        OutboxItems.Add(new OutboxItem(topic, JsonConvert.SerializeObject(message)));
    }
}