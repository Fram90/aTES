using aTES.Common.Shared.Kafka;

namespace aTES.Auth.Kafka;

public static class KafkaDiExtensions
{
    public static void AddKafkaServices(this IServiceCollection services)
    {
        services.AddSingleton<KafkaClientHandle>();
        services.AddSingleton<KafkaDependentProducer<string, string>>();
        services.AddSingleton<KafkaDependentProducer<string, long>>();
    }
}