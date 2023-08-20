using aTES.Accounting.Db;
using aTES.Accounting.Kafka.Models;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace aTES.Accounting.Kafka.Consumers;

public class StreamingUserChanged : BaseConsumer<string, PopugUserStreamingModel>
{
    public StreamingUserChanged(string topicName, string consumerGroup, IConfiguration config, IServiceProvider serviceProvider) : base(topicName, consumerGroup, config, serviceProvider)
    {
    }

    protected override void Consume(PopugUserStreamingModel value, IServiceProvider serviceProvider)
    {
        var ctx = serviceProvider.GetRequiredService<AccountingDbContext>();

        var existing = ctx.Users.FirstOrDefault(c => c.PublicId == value.PublicId);
        if (existing == null)
        {
            var user = new User()
            {
                PublicId = value.PublicId,
                Email = value.Email,
                Role = value.Role
            };
            
            ctx.Users.Add(user);
        }
        else
        {
            existing.Role = value.Role;
        }

        ctx.SaveChanges();
    }
}