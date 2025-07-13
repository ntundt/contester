using contester.Data;
using contester.Features.Attempts;
using contester.Features.Attempts.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IGradeCalculationService
{
    Task<int> CalculateAttemptGrade(Guid attemptId, CancellationToken cancellationToken);
}

public class GradeCalculationService(ApplicationDbContext context) : IGradeCalculationService
{
    public async Task<int> CalculateAttemptGrade(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .ThenInclude(p => p.Contest)
            .ThenInclude(c => c.CommissionMembers)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken) ?? throw new AttemptNotFoundException();
        
        if (attempt.Status != AttemptStatus.Accepted)
        {
            return 0;
        }

        var gradeAdjustments = await context.GradeAdjustments.AsNoTracking()
            .Where(ga => ga.AttemptId == attemptId)
            .ToListAsync(cancellationToken);
        try {
            return ((attempt.Problem.Contest.CommissionMembers.Count - gradeAdjustments.Count) * attempt.Problem.MaxGrade
                + gradeAdjustments.Sum(ga => ga.Grade)) / attempt.Problem.Contest.CommissionMembers.Count;
        } catch (DivideByZeroException) {
            return attempt.Problem.MaxGrade;
        }
    }
}
