using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Attempts;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.GradeAdjustments.Commands;

public class AdjustGradeCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid AttemptId { get; set; }
    public int Grade { get; set; }
    public string Comment { get; set; } = null!;
}

public class AdjustGradeCommandHandler : IRequestHandler<AdjustGradeCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly IClaimService _claimService;

    public AdjustGradeCommandHandler(ApplicationDbContext context, IClaimService claimService)
    {
        _context = context;
        _claimService = claimService;
    }

    public async Task<Unit> Handle(AdjustGradeCommand request, CancellationToken cancellationToken)
    {
        var existingGradeAdjustment = await _context.GradeAdjustments.AsNoTracking()
            .FirstOrDefaultAsync(ga => ga.AttemptId == request.AttemptId
                && ga.UserId == request.UserId, cancellationToken);
        
        if (existingGradeAdjustment != null)
        {
            existingGradeAdjustment.Grade = request.Grade;
            existingGradeAdjustment.Comment = request.Comment;
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        var attempt = await _context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .ThenInclude(p => p.Contest)
            .ThenInclude(c => c.CommissionMembers)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);

        if (attempt == null) throw new Exception("Attempt not found");

        if (attempt.Status != AttemptStatus.Accepted)
            throw new NotifyUserException("Attempt is not accepted. Grade is forced to be 0");

        if (!attempt.Problem.Contest.CommissionMembers.Any(cm => cm.Id == request.UserId))
            throw new NotifyUserException("You're not a commission member");
        
        if (request.Grade < 0 || request.Grade > attempt.Problem.MaxGrade * 2)
            throw new NotifyUserException($"Grade must be between 0 and {attempt.Problem.MaxGrade * 2}");
        
        var gradeAdjustment = new GradeAdjustment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            AttemptId = request.AttemptId,
            Grade = request.Grade,
            Comment = request.Comment
        };

        _context.GradeAdjustments.Add(gradeAdjustment);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}