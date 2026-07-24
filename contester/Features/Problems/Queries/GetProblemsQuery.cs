using System.Text.Json.Serialization;
using AutoMapper;
using contester.Common.MediatR;
using contester.Features.Attempts;
using contester.Features.Common.Exceptions;
using contester.Features.Contests;
using contester.Features.Contests.Policies;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Queries;

public class GetProblemsQuery : IRequest<GetProblemsQueryResult>, IAuthenticatedRequest
{
    public Guid ContestId { get; set; }
    [JsonIgnore]
    public Guid CallerId { get; set; }
}

public class GetProblemsQueryResult
{
    public List<ProblemDto> Problems { get; set; } = null!;
}

public class GetProblemsQueryHandler(
    ApplicationDbContext context,
    ProblemListAccessPolicy problemListAccessPolicy,
    IFileService fileService)
    : IRequestHandler<GetProblemsQuery, GetProblemsQueryResult>
{
    public async Task<GetProblemsQueryResult> Handle(GetProblemsQuery request, CancellationToken cancellationToken)
    {
        var contest = await context.Contests.AsNoTracking()
            .Include(c => c.ParticipantsGroup)
            .Include(c => c.CommissionMembers)
            .FirstOrDefaultAsync(c => c.Id == request.ContestId, cancellationToken);
        
        if (contest is null)
            throw new EntityNotFoundException(typeof(Contest), request.ContestId);

        if (!await problemListAccessPolicy.CanReadAsync(request.CallerId, contest.Id, cancellationToken))
            throw new NotifyUserException("You cannot view this contest's problems");
        
        var problems = await context.Problems.AsNoTracking()
            .Where(p => p.ContestId == request.ContestId)
            .OrderBy(p => p.Ordinal)
            .Select(p => new ProblemDto
            {
                Id = p.Id,
                Name = p.Name,
                Statement = fileService.ReadApplicationDirectoryFileAllText(p.StatementPath),
                OrderMatters = p.OrderMatters,
                FloatMaxDelta = p.FloatMaxDelta,
                CaseSensitive = p.CaseSensitive,
                TimeLimit = p.TimeLimit,
                MaxGrade = p.MaxGrade,
                Ordinal = p.Ordinal,
                SchemaDescriptionId = p.SchemaDescriptionId,

                UsersSolved = p.Attempts
                    .Where(s => s.Status == AttemptStatus.Accepted)
                    .Select(s => s.AuthorId)
                    .Distinct()
                    .Count(),

                IsSolved = p.Attempts
                    .Any(s =>
                        s.AuthorId == request.CallerId &&
                        s.Status == AttemptStatus.Accepted),

                AvailableDbms = p.SchemaDescription.Files
                    .Where(f => !f.HasProblems)
                    .Select(f => f.Dbms)
                    .ToList()
            })
            .ToListAsync(cancellationToken);
        return new GetProblemsQueryResult
        {
            Problems = problems,
        };
    }
}
