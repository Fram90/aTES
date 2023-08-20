using aTES.Accounting.Kafka.Models;

namespace aTES.Accounting.Domain;

public class Account
{
    public int Id { get; set; }
    public Guid PopugPublicId { get; set; }

    public List<PopugTransaction> Transactions { get; set; }
    // public List<AuditLogItem> AuditLog { get; set; }

    public void Charge()
    {
    }

    public void Charge(BillingCycle billingCycle, Guid taskPublicId, decimal chargeAmount)
    {
        var transaction = new PopugTransaction(Id, $"Списание средств за назначение задачи {taskPublicId}",
            TransactionType.Debit, 0, chargeAmount, billingCycle.Id, DateTimeOffset.UtcNow);
        Transactions.Add(transaction);
    }

    public void Pay(BillingCycle billingCycle, Guid taskId, decimal paymentAmount)
    {
        var transaction = new PopugTransaction(Id, $"Начисление средств за выполнение задачи {taskId}",
            TransactionType.Credit, paymentAmount, 0, billingCycle.Id, DateTimeOffset.UtcNow);
        Transactions.Add(transaction);
    }
}