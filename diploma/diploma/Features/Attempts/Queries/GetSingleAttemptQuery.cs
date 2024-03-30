using diploma.Data;
using diploma.Features.Attempts.Exceptions;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Attempts.Queries;

public class GetSingleAttemptQuery : IRequest<SingleAttemptDto>
{
    public Guid AttemptId { get; set; }
    public Guid CallerId { get; set; }
}

public class GetSingleAttemptQueryHandler : IRequestHandler<GetSingleAttemptQuery, SingleAttemptDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly IGradeCalculationService _gradeCalculationService;

    public GetSingleAttemptQueryHandler(ApplicationDbContext context, IPermissionService permissionService, IGradeCalculationService gradeCalculationService)
    {
        _context = context;
        _permissionService = permissionService;
        _gradeCalculationService = gradeCalculationService;
    }

    public async Task<SingleAttemptDto> Handle(GetSingleAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts.AsNoTracking()
            .Include(a => a.Author)
            .Include(a => a.Problem)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);
        if (attempt == null)
        {
            throw new AttemptNotFoundException();
        }

        var userIsCommissionMember = await _context.Contests.AsNoTracking()
            .AnyAsync(c => c.Id == attempt.Problem.ContestId && c.CommissionMembers.Any(cm => cm.Id == request.CallerId), cancellationToken);
        if (attempt.AuthorId != request.CallerId && !await _permissionService.UserHasPermissionAsync(request.CallerId, "ManageAttempts", cancellationToken)
            && !userIsCommissionMember)
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, "ManageAttempts");
        }

        var result = new SingleAttemptDto
        {
            Id = attempt.Id,
            ProblemId = attempt.ProblemId,
            AuthorId = attempt.AuthorId,
            Status = attempt.Status,
            CreatedAt = attempt.CreatedAt,
            AuthorFirstName = attempt.Author.FirstName,
            AuthorLastName = attempt.Author.LastName,
            AuthorPatronymic = attempt.Author.Patronymic,
            Grade = await _gradeCalculationService.CalculateAttemptGrade(attempt.Id, cancellationToken),
            MaxGrade = attempt.Problem.MaxGrade,
            ProblemName = attempt.Problem.Name,
            Solution = await File.ReadAllTextAsync(attempt.SolutionPath, cancellationToken),
            SolutionDbms = attempt.Dbms,
            Originality = attempt.Originality,
            OriginalAttemptId = attempt.OriginalAttemptId ?? Guid.Empty,
        };
        return result;
    }
}
