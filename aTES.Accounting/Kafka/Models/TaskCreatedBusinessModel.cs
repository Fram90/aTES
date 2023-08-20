namespace aTES.Accounting.Kafka.Models;

public class TaskCreatedBusinessModel
{
    public Guid AssigneePublicId { get; set; }
    public Guid TaskPublicId { get; set; }
    public string Description { get; set; }
    public decimal ChargePrice { get; set; }
    public decimal PaymentPrice { get; set; }
}