using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace aTES.Common.Shared.Kafka;

/// <summary>
///     Leverages the injected KafkaClientHandle instance to allow
///     Confluent.Kafka.Message{K,V}s to be produced to Kafka.
/// </summary>
public class KafkaDependentProducer<K, V>
{
    IProducer<K, V> kafkaHandle;
    private ILogger _logger;

    public KafkaDependentProducer(KafkaClientHandle handle, ILogger logger)
    {
        _logger = logger;
        kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();
    }

    /// <summary>
    ///     Asychronously produce a message and expose delivery information
    ///     via the returned Task. Use this method of producing if you would
    ///     like to await the result before flow of execution continues.
    /// <summary>
    public async Task ProduceAsync(string topic, Message<K, V> message)
    {
        await this.kafkaHandle.ProduceAsync(topic, message);
        _logger.LogInformation($"Produced message with Id {message.Key?.ToString()} to topic '{topic}'");
    }

    /// <summary>
    ///     Asynchronously produce a message and expose delivery information
    ///     via the provided callback function. Use this method of producing
    ///     if you would like flow of execution to continue immediately, and
    ///     handle delivery information out-of-band.
    /// </summary>
    public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>> deliveryHandler = null)
        => this.kafkaHandle.Produce(topic, message, deliveryHandler);

    public void Flush(TimeSpan timeout)
        => this.kafkaHandle.Flush(timeout);
}