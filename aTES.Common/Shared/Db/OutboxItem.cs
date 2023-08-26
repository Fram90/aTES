namespace aTES.Common.Shared.Db;

public class OutboxItem
{
    public int Id { get; set; }
    public string Topic { get; set; }
    public string Message { get; set; }
    public DateTimeOffset Added { get; set; }

    private OutboxItem()
    {
    }

    public OutboxItem(string topic, string message)
    {
        Topic = topic;
        Message = message;
        Added = DateTimeOffset.UtcNow;
    }
}