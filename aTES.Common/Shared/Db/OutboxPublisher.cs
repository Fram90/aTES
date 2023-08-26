using System.Data;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace aTES.Common.Shared.Db;

public class OutboxPublisher<TContext> : IInvocable where TContext : OutboxContext<TContext>
{
    private readonly TContext _context;
    private readonly KafkaDependentProducer<string, string> _kafkaDependentProducer;
    private readonly ILogger _logger;

    public OutboxPublisher(TContext context, KafkaDependentProducer<string, string> kafkaDependentProducer, ILogger logger)
    {
        _context = context;
        _kafkaDependentProducer = kafkaDependentProducer;
        _logger = logger;
    }

    public async Task Invoke()
    {
        var outboxItems = _context.OutboxItems.ToList();

        foreach (var outboxItem in outboxItems)
        {
            var message = JsonConvert.DeserializeObject<Message<string, string>>(outboxItem.Message);

            if (message == null)
            {
                _logger.LogError($"Не смогли отправить в кафку сообщение {outboxItem.Message}");
                continue;
            }

            await _kafkaDependentProducer.ProduceAsync(outboxItem.Topic, message);
            _context.OutboxItems.Remove(outboxItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DBConcurrencyException e)
            {
                //в рамках учебного проекта пока забъем, но есть проблема двойной отправки в брокер. Можно навесить блокировку
                // но тогда возможны просадки по производитеьности. Нужно смотреть, что важнее
                _logger.LogWarning(e, "Несколько потоков пытались обработать одно и то же сообщение в аутбоксе. +- норм");
            }
        }
    }
}