namespace aTES.Accounting.BillingCycleProcessing;

public class DailySalarySender
{
    private readonly ILogger _logger;
    

    public DailySalarySender(ILogger logger)
    {
        _logger = logger;
    }

    public void Send(string email, string body)
    {
        //в действительности тут тоже запускаем асинхронное взаимодействие через outbox 
        _logger.LogInformation($"На мейл {email} отправлена заработанная сумма за день. {body}");
    }
}