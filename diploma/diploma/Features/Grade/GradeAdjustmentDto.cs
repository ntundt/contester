namespace diploma.Features.Grade;

public class GradeAdjustmentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AttemptId { get; set; }
    public int Grade { get; set; }
    public string Comment { get; set; } = null!;
    public string CommissionMemberFirstName { get; set; } = null!;
    public string CommissionMemberLastName { get; set; } = null!;
    public string? CommissionMemberPatronymic { get; set; } = null!;
}
