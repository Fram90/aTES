using Task = aTES.TaskTracker.Domain.Task;

namespace aTES.TaskTracker.Kafka.Models;

public class TaskChangedStreamingModel
{
    public Guid AssigneePublicId { get; set; }
    public Guid TaskPublicId { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public decimal ChargePrice { get; set; }
    public decimal PaymentPrice { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    
    public static TaskChangedStreamingModel FromDomain(Task task)
    {
        return new TaskChangedStreamingModel()
        {
            AssigneePublicId = task.PopugPublicId,
            TaskPublicId = task.PublicId,
            Description = task.Description,
            Status = task.Status.ToString(),
            ChargePrice = task.ChargePrice,
            PaymentPrice = task.PaymentPrice,
            CompletedAt = task.CompletedAt
        };
    }
}