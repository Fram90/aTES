namespace aTES.Accounting.Db;

public class StreamedTask
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Description { get; set; }

    public Guid PopugPublicId { get; set; }

    public decimal ChargePrice { get; set; }
    public decimal PaymentPrice { get; set; }
}