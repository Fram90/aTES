using aTES.TaskTracker.Kafka.Consumers;
using Confluent.Kafka;

namespace aTES.TaskTracker.Kafka;

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
    }
}