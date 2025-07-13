using System.ComponentModel.DataAnnotations;
using contester.Data.Common;
using contester.Features.Attempts;
using contester.Features.Users;

namespace contester.Features.Grade;

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
