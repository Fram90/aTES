using aTES.Accounting.Db;
using aTES.Accounting.Domain;
using aTES.Accounting.Kafka.Events;
using aTES.Common;
using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;

namespace aTES.Accounting.BillingCycleProcessing;

public class CloseBillingCycleJob : IInvocable
{
    private readonly AccountingDbContext _dbContext;
    private readonly DailySalarySender _salarySender;

    public CloseBillingCycleJob(AccountingDbContext dbContext, DailySalarySender salarySender)
    {
        _dbContext = dbContext;
        _salarySender = salarySender;
    }

    public async Task Invoke()
    {
        var currentBillingCycle = _dbContext.GetCurrentBillingCycle();
        if (currentBillingCycle == null)
        {
            throw new Exception("Нет текущего открытого биллинг цикла");
        }

        //аккаунты с транзакциями текущего незакрытого биллинг цикла
        var accountsWithTransactions = _dbContext.Accounts
            .Include(x => x.Transactions)
            .Where(c => c.Transactions.Where(v => v.BillingCycleId == currentBillingCycle.Id)
                .All(t => t.Type != TransactionType.BillingCycleClose));

        foreach (var withTransaction in accountsWithTransactions)
        {
            var transaction = withTransaction.CloseCycle(currentBillingCycle.Id);

            var user = _dbContext.Users.FirstOrDefault(x => x.PublicId == withTransaction.PopugPublicId);
            if (user == null)
            {
                throw new Exception("retry");
            }

            if (transaction.CreditValue > 0)
                _salarySender.Send(user.Email, $"Выплата {transaction.CreditValue.ToString()} денег. Ты молодец");

            var eventBody = new AccountTransactionCreated()
            {
                Type = transaction.Type.ToString(),
                Description = transaction.Description,
                Issued = transaction.Issued,
                CreditAmount = transaction.CreditValue,
                DebitAmount = transaction.DebitValue,
                AccountPublicId = withTransaction.PopugPublicId,
                TransactionId = transaction.PublicId
            };
            var message = BaseMessage<AccountTransactionCreated>.Create(transaction.PublicId.ToString(), "account.transaction.created.v1", eventBody);
            _dbContext.Produce("be-account-transaction-created", message);

            //
            // var billingCycleClosed = new BillingCycleClosed()
            // {
            //     PopugPublicId = withTransaction.PopugPublicId,
            //     BillingCycle = currentBillingCycle.Id
            // };
            // var cycleClosedMessage = BaseMessage<BillingCycleClosed>.Create(Guid.NewGuid().ToString(), "be.billing.cycle.closed.v1", billingCycleClosed);
            // _dbContext.Produce("be-billing-cycle-closed", cycleClosedMessage);

            // каждая итерация - закрытие биллинг цикла для одного попуга. Так что обновляем аккаунт/записываем события в аутбокс для каждого попуга
            await _dbContext.SaveChangesAsync();
            //если будем ретраить, то второй раз в те же аккаунты не попадем,
            //т.к. есть проверки на наличие транзакии с закрытием биллинг цикла в текущем цикле 
        }

        //когда обработали всех попугов, закроем биллинг цикл и создадим новый. Наверное правильно было бы завернуть это в отдельный асинхронный обработчик
        currentBillingCycle.Close();

        var newCycle = CreateNewBillingCycle();
        _dbContext.BillingCycles.Add(newCycle);
        await _dbContext.SaveChangesAsync();
    }

    private static BillingCycle CreateNewBillingCycle()
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var todayEnd = new DateTimeOffset(now.Year, now.Month, now.Day, 23, 59, 59, TimeSpan.Zero);
        var newCycle = new BillingCycle(todayStart, todayEnd, BillingCycleStatus.Open);
        return newCycle;
    }
}