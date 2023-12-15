namespace diploma.Data.Common;

public class AuditableEntity
{
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime UpdatedAt { get; set; }
}