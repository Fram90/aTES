using aTES.Accounting.Db;
using aTES.Accounting.Domain;
using aTES.Accounting.Domain.Services;
using aTES.Accounting.Kafka.Models;
using aTES.Accounting.Kafka.StreamingModels;
using aTES.Common;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace aTES.Accounting.Kafka.Consumers;

/// <summary>
/// Когда таск переназначен, списать с попуга деньги
/// </summary>
public class BusinessTaskShuffledConsumer : BaseConsumer<string, TaskShuffledBusinessModel>
{
    public BusinessTaskShuffledConsumer(string topicName, string consumerGroup, IConfiguration config, IServiceProvider serviceProvider)
        : base(topicName, consumerGroup, config, serviceProvider)
    {
    }

    protected override void Consume(TaskShuffledBusinessModel value, IServiceProvider serviceProvider)
    {
        var accountService = serviceProvider.GetRequiredService<AccountService>();

        accountService.Charge(value.AssigneePublicId, value.TaskPublicId, value.ChargePrice);
    }
}