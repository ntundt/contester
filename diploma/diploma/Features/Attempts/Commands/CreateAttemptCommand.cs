using System.Text.RegularExpressions;
using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Problems.Exceptions;
using diploma.Features.Scoreboard.Services;
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

public partial class CreateAttemptCommandHandler : IRequestHandler<CreateAttemptCommand, AttemptDto>
{
    [GeneratedRegex("\\s+")]
    private static partial Regex SpaceCharRegex();

    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly ISolutionRunnerService _solutionRunnerService;
    private readonly ScoreboardUpdateNotifier _scoreboardUpdateNotifier;

    public CreateAttemptCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, IFileService fileService,
        ISolutionRunnerService solutionRunnerService, ScoreboardUpdateNotifier notifier)
    {
        _context = context;
        _directoryService = directoryService;
        _fileService = fileService;
        _solutionRunnerService = solutionRunnerService;
        _scoreboardUpdateNotifier = notifier;
    }

    private static string PreprocessSolution(string solution)
    {
        return SpaceCharRegex().Replace(solution.Trim().ToLower(), " ");
    }

    private class OriginalityCheckResult
    {
        public int? Originality { get; set; }
        public Guid OriginalAttemptId { get; set; }
    }
    private async Task<OriginalityCheckResult> CheckOriginalityAsync(Guid problemId, string solution, Guid currentAttemptId,
        Guid authorId, CancellationToken cancellationToken)
    {
        var attempts = await _context.Attempts.AsNoTracking()
            .Where(a => a.ProblemId == problemId && a.Id != currentAttemptId && a.AuthorId != authorId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
        if (attempts.Count == 0)
        {
            return new OriginalityCheckResult
            {
                Originality = solution.Length,
                OriginalAttemptId = Guid.Empty,
            };
        }

        var fastenshtein = new Fastenshtein.Levenshtein(PreprocessSolution(solution));
        var originalities = await Task.WhenAll(attempts.Select(async a => {
            int originality;
            try {
                var fileContents = await _fileService.ReadApplicationDirectoryFileAllTextAsync(a.SolutionPath, cancellationToken);
                originality = fastenshtein.DistanceFrom(PreprocessSolution(fileContents));
            } catch (Exception) {
                originality = int.MaxValue;
            }
            return new { a.Id, Originality = originality };
        }).ToList());

        var minOriginality = originalities.Min(o => o.Originality);
        var originalAttemptId = originalities.First(o => o.Originality == minOriginality).Id;

        return new OriginalityCheckResult
        {
            Originality = minOriginality,
            OriginalAttemptId = originalAttemptId,
        };
    }

    public async Task<AttemptDto> Handle(CreateAttemptCommand request, CancellationToken cancellationToken)
    {
        var problem = await _context.Problems.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        
        var attempt = new Attempt
        {
            Id = Guid.NewGuid(),
            ProblemId = request.ProblemId,
            AuthorId = request.AuthorId,
            Dbms = request.Dbms,
            Status = AttemptStatus.Pending,
        };
        attempt.SolutionPath = _directoryService.GetAttemptRelativePath(attempt.Id);
        await _fileService.SaveAttemptToFileAsync(attempt.Id, request.Solution, cancellationToken);

        _context.Attempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        var originalityCheckResult 
            = await CheckOriginalityAsync(request.ProblemId, request.Solution, attempt.Id, request.AuthorId, cancellationToken);

        if (originalityCheckResult.OriginalAttemptId != Guid.Empty)
        {
            attempt.Originality = originalityCheckResult.Originality;
            attempt.OriginalAttemptId = originalityCheckResult.OriginalAttemptId;
            _context.Attempts.Update(attempt);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
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

        await _scoreboardUpdateNotifier.SendScoreboardUpdate(problem.ContestId);

        return attemptDto;
    }
}