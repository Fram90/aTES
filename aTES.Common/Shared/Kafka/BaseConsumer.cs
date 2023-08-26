using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace aTES.Common.Shared.Kafka;

public abstract class BaseConsumer<TKey, TValue> : BackgroundService
{
    private readonly string _topic;
    private readonly IConsumer<TKey, string> _kafkaConsumer;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    protected abstract void Consume(TValue value, IServiceProvider serviceProvider);

    public BaseConsumer(string topicName, string consumerGroup, IConfiguration config, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger>();
        var consumerConfig = new ConsumerConfig();
        config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
        consumerConfig.GroupId = consumerGroup;
        this._topic = topicName;
        this._kafkaConsumer = new ConsumerBuilder<TKey, string>(consumerConfig).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private void StartConsumerLoop(CancellationToken cancellationToken)
    {
        _kafkaConsumer.Subscribe(this._topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var cr = this._kafkaConsumer.Consume(cancellationToken);


                using (var scope = _serviceProvider.CreateScope())
                {
                    var payload = JsonConvert.DeserializeObject<BasePayload<TValue>>(cr.Message.Value)!
                        .Payload;

                    Consume(payload, scope.ServiceProvider);
                    var messageId = cr.Message.Key?.ToString() ?? "Null";
                    _logger.LogInformation($"Consumed message {messageId} in topic {_topic}");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                _logger.LogError(new EventId(), e, $"Consumer error {e}");

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(new EventId(), e, $"Unexpected error: {e}");
                break;
            }
        }
    }

    public override void Dispose()
    {
        this._kafkaConsumer.Close(); // Commit offsets and leave the group cleanly.
        this._kafkaConsumer.Dispose();

        base.Dispose();
    }
}