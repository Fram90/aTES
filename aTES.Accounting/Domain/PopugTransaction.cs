namespace aTES.Accounting.Domain;

public class PopugTransaction
{
    public int Id { get; private set; }
    public int AccountId { get; private set; }
    public string Description { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal CreditValue { get; private set; }
    public decimal DebitValue { get; private set; }

    public int BillingCycleId { get; private set; }
    public DateTimeOffset Issued { get; private set; }

    public virtual Account OwnerAccount { get; private set; }
    public virtual BillingCycle BillingCycle { get; private set; }

    private PopugTransaction()
    {
    }

    public PopugTransaction(int accountId, string description, TransactionType type, decimal creditValue,
        decimal debitValue, int billingCycleId, DateTimeOffset issued)
    {
        AccountId = accountId;
        Description = description;
        Type = type;
        CreditValue = creditValue;
        DebitValue = debitValue;
        BillingCycleId = billingCycleId;
        Issued = issued;
    }
}

public class BillingCycle
{
    public int Id { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public BillingCycleStatus Status { get; private set; }

    public virtual List<PopugTransaction> Transactions { get; private set; }

    private BillingCycle()
    {
    }

    public BillingCycle( DateTimeOffset startDate, DateTimeOffset endDate, BillingCycleStatus status)
    {
        StartDate = startDate.ToUniversalTime();
        EndDate = endDate.ToUniversalTime();
        Status = status;
    }

    public void Close()
    {
        Status = BillingCycleStatus.Closed;
    }
}

public enum TransactionType
{
    Credit,
    Debit,
    BillingCycleClose
}

public enum BillingCycleStatus
{
    Open,
    Closed
}