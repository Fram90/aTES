namespace aTES.TaskTracker.Domain;

public class Task
{
    public int Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Description { get; private set; }

    public Guid PopugPublicId { get; private set; }

    public decimal ChargePrice { get; private set; }
    public decimal PaymentPrice { get; private set; }

    public TaskState Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private Task()
    {
    }

    public Task(Guid publicId, string description, Guid popugPublicId, decimal chargePrice, decimal paymentPrice)
    {
        PublicId = publicId;
        Description = description;
        PopugPublicId = popugPublicId;
        ChargePrice = chargePrice;
        PaymentPrice = paymentPrice;

        Status = TaskState.Open;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AssignTo(Guid popugId)
    {
        PopugPublicId = popugId;
    }

    public void Close()
    {
        Status = TaskState.Closed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

public enum TaskState
{
    Open,
    Closed
}