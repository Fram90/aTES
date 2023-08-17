namespace aTES.Accounting.Db;

public class User
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public string Role { get; set; }
}