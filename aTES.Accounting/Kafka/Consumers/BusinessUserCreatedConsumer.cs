using aTES.Accounting.Db;
using aTES.Accounting.Domain;
using aTES.Accounting.Kafka.Models;
using aTES.Accounting.Kafka.StreamingModels;
using aTES.Common;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace aTES.Accounting.Kafka.Consumers;

/// <summary>
/// Когда создан пользователь, создать для него счет
/// </summary>
public class BusinessUserCreatedConsumer : BaseConsumer<string, PopugUserCreatedModel>
{
    public BusinessUserCreatedConsumer(string topicName, string consumerGroup, IConfiguration config, IServiceProvider serviceProvider) : base(topicName, consumerGroup, config, serviceProvider)
    {
    }

    protected override void Consume(PopugUserCreatedModel value, IServiceProvider serviceProvider)
    {
        var ctx = serviceProvider.GetRequiredService<AccountingDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger>();

        var existing = ctx.Accounts.FirstOrDefault(c => c.PopugPublicId == value.PublicId);
        if (existing == null)
        {
            var account = new Account()
            {
                PopugPublicId = value.PublicId
            };
            ctx.Accounts.Add(account);
            ctx.SaveChanges();

            logger.LogInformation($"Created account for new User: {value.PublicId}");
        }
    }
}