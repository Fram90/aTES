namespace aTES.Accounting.Domain;

public class Account
{
    public int Id { get; set; }
    public Guid PopugPublicId { get; set; }

    public decimal CurrentBalance { get; set; }
    
    public List<AuditLogItem> AuditLog { get; set; }
}