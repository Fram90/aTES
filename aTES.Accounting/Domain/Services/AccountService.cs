using aTES.Accounting.Db;
using aTES.Accounting.Kafka.Events;
using aTES.Common;
using aTES.Common.Shared.Kafka;
using Confluent.Kafka;

namespace aTES.Accounting.Domain.Services;

public class AccountService
{
    private readonly AccountingDbContext _ctx;
    private readonly ILogger _logger;
    private readonly KafkaDependentProducer<Null, string> _producer;

    public AccountService(AccountingDbContext ctx, ILogger logger, KafkaDependentProducer<Null, string> producer)
    {
        this._ctx = ctx;
        _logger = logger;
        _producer = producer;
    }

    public void Charge(Guid popugId, Guid taskId, decimal chargeAmount)
    {
        var account = _ctx.Accounts.FirstOrDefault(c => c.PopugPublicId == popugId);
        if (account == null)
        {
            //todo retry
            throw new Exception("пришло событие на списание средств, а счет для попуга еще не создан");
        }

        var billingCycle = _ctx.GetOrAddCurrentBillingCycle();

        var transaction = account.Charge(billingCycle, taskId, chargeAmount);
        _ctx.SaveChanges();

        _logger.LogInformation($"Списали средства с попуга {account.PopugPublicId} за назначение задачи {taskId}");

        var transactionEventModel = new AccountTransactionCreated()
        {
            Type = TransactionType.Debit.ToString(),
            Description = transaction.Description,
            Issued = transaction.Issued,
            CreditAmount = transaction.CreditValue,
            DebitAmount = transaction.DebitValue,
            AccountPublicId = account.PopugPublicId
        };

        _producer.Produce("be-account-transaction-created", new Message<Null, string>()
        {
            Value = BaseMessage<AccountTransactionCreated>.Create("account.transaction.created.v1", transactionEventModel).ToJson()
        });
    }

    public void Pay(Guid popugId, Guid taskId, decimal payAmount)
    {
        var account = _ctx.Accounts.FirstOrDefault(c => c.PopugPublicId == popugId);
        if (account == null)
        {
            //todo retry
            throw new Exception("пришло событие на пополнение счета, а счет для попуга еще не создан");
        }

        var billingCycle = _ctx.GetOrAddCurrentBillingCycle();

        var transaction = account.Pay(billingCycle, taskId, payAmount);
        _ctx.SaveChanges();

        _logger.LogInformation($"Зачислили средства попугу {account.PopugPublicId} за выполнение задачи {taskId}");

        var transactionEventModel = new AccountTransactionCreated()
        {
            Type = TransactionType.Debit.ToString(),
            Description = transaction.Description,
            Issued = transaction.Issued,
            CreditAmount = transaction.CreditValue,
            DebitAmount = transaction.DebitValue,
            AccountPublicId = account.PopugPublicId
        };

        _producer.Produce("be-account-transaction-created", new Message<Null, string>()
        {
            Value = BaseMessage<AccountTransactionCreated>.Create("account.transaction.created.v1", transactionEventModel).ToJson()
        });
    }
}