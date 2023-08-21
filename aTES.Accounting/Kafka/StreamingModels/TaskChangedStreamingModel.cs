namespace aTES.Accounting.Kafka.StreamingModels;

public class TaskChangedStreamingModel
{
    public Guid AssigneePublicId { get; set; }
    public Guid TaskPublicId { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public decimal ChargePrice { get; set; }
    public decimal PaymentPrice { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}