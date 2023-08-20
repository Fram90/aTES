using System.Reflection;
using aTES.Accounting.Kafka.Consumers;
using aTES.Accounting.Kafka.Models;
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
        services.AddConsumer<StreamingUserChanged, string, PopugUserStreamingModel>("stream-user-lifecycle", "accounting-stream-user-lifecycle");
        services.AddConsumer<BusinessUserCreatedConsumer, string, PopugUserCreatedModel>("be-user-created", "accounting-be-user-created");
        
        services.AddConsumer<BusinessTaskCreatedConsumer, string, TaskCreatedBusinessModel>("be-task-created", "accounting-be-task-created");
        services.AddConsumer<BusinessTaskShuffledConsumer, string, TaskShuffledBusinessModel>("be-task-shuffled", "accounting-be-task-shuffled");
    }

    private static void AddConsumer<TConsumer, TKey, TMessage>(this IServiceCollection services, string topicName, string groupId) where TConsumer : BaseConsumer<TKey, TMessage>
    {
        services.AddHostedService<TConsumer>(provider =>
            (TConsumer)Activator.CreateInstance(typeof(TConsumer), new object[] { topicName, groupId, provider.GetRequiredService<IConfiguration>(), provider })!);
    }
}