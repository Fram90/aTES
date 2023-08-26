namespace aTES.Accounting.Domain;

public class Account
{
    public int Id { get; set; }
    public Guid PopugPublicId { get; set; }

    public List<PopugTransaction> Transactions { get; set; }

    public decimal GetBalanceForCycle(int billingCycleId)
    {
        var currentCycleTransactions = Transactions.Where(x => x.BillingCycleId == billingCycleId).ToList();
        var total = currentCycleTransactions.Sum(x => x.CreditValue) - currentCycleTransactions.Sum(x => x.DebitValue);
        return total;
    }

    public void Charge()
    {
    }

    public PopugTransaction Charge(BillingCycle billingCycle, Guid taskPublicId, decimal chargeAmount)
    {
        var transaction = new PopugTransaction(Id, $"Списание средств за назначение задачи {taskPublicId}",
            TransactionType.Debit, 0, chargeAmount, billingCycle.Id, DateTimeOffset.UtcNow);
        Transactions.Add(transaction);

        return transaction;
    }

    public PopugTransaction Pay(BillingCycle billingCycle, Guid taskId, decimal paymentAmount)
    {
        var transaction = new PopugTransaction(Id, $"Начисление средств за выполнение задачи {taskId}",
            TransactionType.Credit, paymentAmount, 0, billingCycle.Id, DateTimeOffset.UtcNow);
        Transactions.Add(transaction);

        return transaction;
    }

    public PopugTransaction CloseCycle(int billingCycleId)
    {
        var total = GetBalanceForCycle(billingCycleId);
        var credit = total > 0 ? total : 0;
        var debit = total < 0 ? total : 0;

        var transaction = new PopugTransaction(Id, $"Закрытие биллинг цикла", TransactionType.BillingCycleClose, credit, debit, billingCycleId, DateTimeOffset.UtcNow);
        Transactions.Add(transaction);

        return transaction;
    }
}