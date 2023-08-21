namespace aTES.Accounting.Kafka.Models;

public class TaskCompletedBusinessModel
{
    public Guid AssigneePublicId { get; set; }
    public Guid TaskPublicId { get; set; }
    /// <summary>
    /// по идее можно и без этого, и данные брать из стриминга. Но выглядит сложнее - нужно лезть в базу.
    /// Положить цену в ивент несложно
    /// </summary>
    public decimal ChargePrice { get; set; }
}