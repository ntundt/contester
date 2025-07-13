using diploma.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Grade.Queries;

public class GetGradeAdjustmentsQuery : IRequest<List<GradeAdjustmentDto>>
{
    public Guid AttemptId { get; set; }
}

public class GetGradeAdjustmentsQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetGradeAdjustmentsQuery, List<GradeAdjustmentDto>>
{
    public async Task<List<GradeAdjustmentDto>> Handle(GetGradeAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var gradeAdjustments = await context.GradeAdjustments.AsNoTracking()
            .Include(ga => ga.User)
            .Where(ga => ga.AttemptId == request.AttemptId)
            .ToListAsync(cancellationToken);

        return gradeAdjustments.Select(ga => new GradeAdjustmentDto
        {
            Id = ga.Id,
            UserId = ga.UserId,
            AttemptId = ga.AttemptId,
            Grade = ga.Grade,
            Comment = ga.Comment,
            CommissionMemberFirstName = ga.User.FirstName,
            CommissionMemberLastName = ga.User.LastName,
            CommissionMemberPatronymic = ga.User.Patronymic
        }).ToList();
    }
}