using aTES.Accounting.Domain.Services;
using aTES.Accounting.Kafka.Models;
using aTES.Common.Shared.Kafka;

namespace aTES.Accounting.Kafka.Consumers;

/// <summary>
/// Когда таск назначен, списать с попуга деньги
/// </summary>
public class BusinessTaskCreatedConsumer : BaseConsumer<string, TaskCreatedBusinessModel>
{
    protected override void Consume(TaskCreatedBusinessModel value, IServiceProvider serviceProvider)
    {
        var accountService = serviceProvider.GetRequiredService<AccountService>();

       accountService.Charge(value.AssigneePublicId, value.TaskPublicId, value.ChargePrice);
       
    }

    public BusinessTaskCreatedConsumer(string topicName, string consumerGroup, IConfiguration config, IServiceProvider serviceProvider) : base(topicName, consumerGroup, config, serviceProvider)
    {
    }
}