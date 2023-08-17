using aTES.Accounting.Kafka.Consumers;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;

namespace aTES.Accounting.Kafka;

public static class KafkaDiExtensions
{
    public static void AddKafkaServices(this IServiceCollection services)
    {
        services.AddSingleton<KafkaClientHandle>();
        services.AddSingleton<KafkaDependentProducer<Null, string>>();
    }

    public static void AddConsumers(this IServiceCollection services)
    {
        services.AddHostedService<StreamingUserChanged>();
        services.AddHostedService<StreamingTaskChanged>();
    }
}