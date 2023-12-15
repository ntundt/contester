using diploma.Data;
using diploma.Features.Problems.Exceptions;
using diploma.Features.Users.Exceptions;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Attempts.Commands;

public class CreateAttemptCommand : IRequest<AttemptDto>
{
    public Guid ProblemId { get; set; }
    public string Solution { get; set; } = null!;
    public string Dbms { get; set; } = null!;
    public Guid AuthorId { get; set; }
}

public class CreateAttemptCommandHandler : IRequestHandler<CreateAttemptCommand, AttemptDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly ISolutionRunnerService _solutionRunnerService;

    public CreateAttemptCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, ISolutionRunnerService solutionRunnerService)
    {
        _context = context;
        _directoryService = directoryService;
        _solutionRunnerService = solutionRunnerService;
    }

    public async Task<AttemptDto> Handle(CreateAttemptCommand request, CancellationToken cancellationToken)
    {
        var problem = await _context.Problems.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.AuthorId, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException();
        }
        
        var attempt = new Attempt
        {
            Id = Guid.NewGuid(),
            ProblemId = request.ProblemId,
            AuthorId = request.AuthorId,
            Dbms = request.Dbms,
            Status = AttemptStatus.Pending,
        };
        attempt.SolutionPath = _directoryService.GetAttemptPath(attempt.Id);
        await _directoryService.SaveAttemptToFileAsync(attempt.Id, request.Solution, cancellationToken);

        _context.Attempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);
        
        var (status, error) = await _solutionRunnerService.RunAsync(attempt.Id, cancellationToken);

        var attemptDto = new AttemptDto
        {
            Id = attempt.Id,
            ProblemId = attempt.ProblemId,
            AuthorId = attempt.AuthorId,
            Status = status,
            CreatedAt = attempt.CreatedAt,
            ErrorMessage = error,
        };
        
        attempt.Status = status;
        attempt.ErrorMessage = error;
        _context.Attempts.Update(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        return attemptDto;
    }
}