namespace aTES.Auth.Kafka.Models;

public class PopugUserStreamingModel
{
    public Guid PublicId { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
}