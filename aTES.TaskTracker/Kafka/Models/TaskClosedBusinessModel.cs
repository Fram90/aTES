using Task = aTES.TaskTracker.Domain.Task;

namespace aTES.TaskTracker.Kafka.Models;

public class TaskClosedBusinessModel
{
    public Guid AssigneePublicId { get; set; }
    public Guid TaskPublicId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public static TaskClosedBusinessModel FromDomain(Task task)
    {
        return new TaskClosedBusinessModel()
        {
            AssigneePublicId = task.PopugPublicId,
            TaskPublicId = task.PublicId,
            CompletedAt = task.CompletedAt
        };
    }
}

public class TaskShuffledBusinessModel
{
    public Guid NewAssigneePublicId { get; set; }
    public Guid TaskPublicId { get; set; }

    public static TaskShuffledBusinessModel FromDomain(Task task)
    {
        return new TaskShuffledBusinessModel()
        {
            NewAssigneePublicId = task.PopugPublicId,
            TaskPublicId = task.PublicId,
        };
    }
}