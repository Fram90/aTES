namespace aTES.Accounting.Domain;

public class AuditLogItem
{
    public int Id { get; set; }
    public string Description { get; set; }
    public decimal? BalanceDelta { get; set; }
    public DateTimeOffset Issued { get; set; }

    public int AccountId { get; set; }
    public virtual Account OwnerAccount { get; set; }
}