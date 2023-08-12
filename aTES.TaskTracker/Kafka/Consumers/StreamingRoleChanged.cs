using aTES.TaskTracker.Db;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace aTES.TaskTracker.Kafka.Consumers;

public class StreamingRoleChanged : BackgroundService
{
    private readonly string _topic;
    private readonly IConsumer<Null, string> _kafkaConsumer;

    private readonly IServiceProvider _serviceProvider;

    public StreamingRoleChanged(IConfiguration config, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var consumerConfig = new ConsumerConfig();
        config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
        this._topic = "stream-user-role-changed";
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

                var newUser = JsonConvert.DeserializeObject<User>(cr.Message.Value)!;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var ctx = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();

                    var existing = ctx.Users.FirstOrDefault(x => x.PublicId == newUser.PublicId);
                    if (existing == null)
                    {
                        Console.WriteLine($"[STREAM] user with publicId {newUser.PublicId} wasn't found in streamed data. Skipping message");
                        break;
                    }
                    
                    existing.Role = newUser.Role;
                    ctx.SaveChanges();
                }

                // Handle message...
                Console.WriteLine($"[STREAM] role for user {newUser.PublicId} changed to {newUser.Role}");
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