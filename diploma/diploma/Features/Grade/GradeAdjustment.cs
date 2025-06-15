using System.ComponentModel.DataAnnotations;
using diploma.Data.Common;
using diploma.Features.Attempts;
using diploma.Features.Users;

namespace diploma.Features.Grade;

public class GradeAdjustment : AuditableEntity
{
    public Guid Id { get; set; }
    public User User { get; set; } = null!;
    public Guid UserId { get; set; }
    public Attempt Attempt { get; set; } = null!;
    public Guid AttemptId { get; set; }
    public int Grade { get; set; }
    [MaxLength(500)]
    public string Comment { get; set; } = null!;
}
