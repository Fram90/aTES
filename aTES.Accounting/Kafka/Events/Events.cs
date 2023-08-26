namespace aTES.Accounting.Kafka.Events;

public class AccountTransactionCreated
{
    public Guid TransactionId { get; set; }
    public Guid AccountPublicId { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public DateTimeOffset Issued { get; set; }
}