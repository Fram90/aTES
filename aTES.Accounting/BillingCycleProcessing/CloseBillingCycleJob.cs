using aTES.Accounting.Db;
using aTES.Accounting.Domain;
using aTES.Accounting.Kafka.Events;
using aTES.Common;
using Confluent.Kafka;
using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;

namespace aTES.Accounting.BillingCycleProcessing;

public class CloseBillingCycleJob : IInvocable
{
    private AccountingDbContext _dbContext;
    private DailySalarySender _salarySender;

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
            throw new Exception("Уже закрыт");
        }

        //аккаунты с транзакиями текущего незакрытого биллинг цикла
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

            _dbContext.Produce("be-account-transaction-created", new Message<string, string>()
            {
                Value = BaseMessage<AccountTransactionCreated>.Create("account.transaction.created.v1",
                    new AccountTransactionCreated()
                    {
                        Type = transaction.Type.ToString(),
                        Description = transaction.Description,
                        Issued = transaction.Issued,
                        CreditAmount = transaction.CreditValue,
                        DebitAmount = transaction.DebitValue,
                        AccountPublicId = withTransaction.PopugPublicId
                    }).ToJson()
            });


            _dbContext.Produce("be-billing-cycle-closed", new Message<string, string>()
            {
                Value = BaseMessage<BillingCycleClosed>.Create("be.billing.cycle.closed.v1", new BillingCycleClosed()
                {
                    PopugPublicId = withTransaction.PopugPublicId,
                    BillingCycle = currentBillingCycle.Id
                }).ToJson()
            });

            //если будем ретраить, то второй раз в те же аккаунты не попадем,
            //т.к. есть проверки на наличие транзакии с закрытием биллинг цикла в текущем цикле 
            await _dbContext.SaveChangesAsync();
        }
    }
}