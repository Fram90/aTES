using aTES.Auth.Models;
using Confluent.Kafka;

namespace aTES.Auth.Kafka;

public static class KafkaDiExtensions
{
    public static void AddKafkaServices(this IServiceCollection services)
    {
        services.AddSingleton<KafkaClientHandle>();
        services.AddSingleton<KafkaDependentProducer<Null, string>>();
        services.AddSingleton<KafkaDependentProducer<string, long>>();
    }
}