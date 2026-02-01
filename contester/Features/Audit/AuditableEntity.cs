namespace contester.Features.Audit;

public class AuditableEntity
{
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime UpdatedAt { get; set; }
}