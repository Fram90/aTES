namespace aTES.Accounting.Kafka.Models;

public class PopugUserCreatedModel
{
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
}