using Task = aTES.TaskTracker.Domain.Task;

namespace aTES.TaskTracker.Kafka.Models;

public class TaskCreatedBusinessModel
{
    public Guid AssigneePublicId { get; set; }
    public Guid TaskPublicId { get; set; }
    public string Description { get; set; }
    public decimal ChargePrice { get; set; }
    public decimal PaymentPrice { get; set; }

    public static TaskCreatedBusinessModel FromDomain(Task task)
    {
        return new TaskCreatedBusinessModel()
        {
            AssigneePublicId = task.PopugPublicId,
            TaskPublicId = task.PublicId,
            Description = task.Description,
            ChargePrice = task.ChargePrice,
            PaymentPrice = task.PaymentPrice,
        };
    }
}