using aTES.Accounting.Domain.Services;
using aTES.Accounting.Kafka.Models;
using aTES.Common.Shared.Kafka;

namespace aTES.Accounting.Kafka.Consumers;

/// <summary>
/// Когда таск назначен, списать с попуга деньги
/// </summary>
public class BusinessTaskCompletedConsumer : BaseConsumer<string, TaskCompletedBusinessModel>
{
    protected override void Consume(TaskCompletedBusinessModel value, IServiceProvider serviceProvider)
    {
        var accountService = serviceProvider.GetRequiredService<AccountService>();

        accountService.Pay(value.AssigneePublicId, value.TaskPublicId, value.ChargePrice);
    }

    public BusinessTaskCompletedConsumer(string topicName, string consumerGroup, IConfiguration config, IServiceProvider serviceProvider) : base(topicName, consumerGroup, config, serviceProvider)
    {
    }
}