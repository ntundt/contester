namespace diploma.Data;

public class Audit
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public DateTime Date { get; set; }
    public string OldValues { get; set; } = null!;
    public string NewValues { get; set; } = null!;
}