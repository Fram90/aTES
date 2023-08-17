using aTES.Accounting.Db;
using aTES.Accounting.Kafka.StreamingModels;
using aTES.Common;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace aTES.Accounting.Kafka.Consumers;

public class StreamingTaskChanged : BackgroundService
{
    private readonly string _topic;
    private readonly IConsumer<Null, string> _kafkaConsumer;

    private readonly IServiceProvider _serviceProvider;

    public StreamingTaskChanged(IConfiguration config, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var consumerConfig = new ConsumerConfig();
        config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
        this._topic = "stream-task-lifecycle";
        this._kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
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

                var task = JsonConvert.DeserializeObject<BaseMessage<TaskChangedStreamingModel>>(cr.Message.Value)!.Payload;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();

                    var existing = ctx.Tasks.FirstOrDefault(c => c.PublicId == task.TaskPublicId);
                    if (existing == null)
                    {
                        ctx.Add(task);
                    }
                    else
                    {
                        existing.Description = task.Description;
                        existing.PopugPublicId = task.AssigneePublicId;
                    }

                    ctx.SaveChanges();
                }

                // Handle message...
                Console.WriteLine($"[STREAM] Synced task with publicId: {task.TaskPublicId}");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
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