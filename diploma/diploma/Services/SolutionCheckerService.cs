using System.Data.Common;
using diploma.Application;
using diploma.Application.Extensions;
using diploma.Data;
using diploma.Features.Attempts;
using Microsoft.EntityFrameworkCore;

namespace diploma.Services;

public interface ISolutionCheckerService
{
    Task<DbDataReader> GetExpectedSolutionResult(Guid problemId, CancellationToken cancellationToken);
    Task<(DbDataReader, string?)> GetSolutionResult(Guid attemptId, CancellationToken cancellationToken);
    Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken);   
}

public class SolutionCheckerService : ISolutionCheckerService
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    
    public SolutionCheckerService(ApplicationDbContext context, IDirectoryService directoryService, IFileService fileService,
        IConfiguration configuration)
    {
        _context = context;
        _directoryService = directoryService;
        _fileService = fileService;
        _configuration = configuration;
    }

    public async Task<DbDataReader> GetExpectedSolutionResult(Guid problemId, CancellationToken cancellationToken)
    {
        var problem = await _context.Problems.AsNoTracking()
            .Include(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(p => p.Id == problemId, cancellationToken);
        if (problem is null) throw new ApplicationException("Problem with the GUID specified not found");

        var dbmsAdapter = new DbmsAdapterFactory(_configuration).Create(problem.SolutionDbms);

        var schemaDescriptionFile = problem.SchemaDescription.Files.FirstOrDefault(f => f.Dbms == problem.SolutionDbms);
        if (schemaDescriptionFile is null) throw new ApplicationException("Schema description file for expected solution does not exist");
        
        var (schemaDescription, solution) = await TaskEx.WhenAll(
            _fileService.ReadApplicationDirectoryFileAllTextAsync(schemaDescriptionFile.FilePath, cancellationToken),
            _fileService.ReadApplicationDirectoryFileAllTextAsync(problem.SolutionPath, cancellationToken)
        );
        
        DbDataReader reader;
        await dbmsAdapter.GetLockAsync(cancellationToken);
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaAsync(schemaDescription, cancellationToken);
            reader = await dbmsAdapter.ExecuteQueryAsync(solution, cancellationToken);
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }

        return reader;
    }
    
    public async Task<(DbDataReader, string?)> GetSolutionResult(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .ThenInclude(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);
        if (attempt is null) throw new ApplicationException("Attempt with the GUID specified not found");

        var dbmsAdapter = new DbmsAdapterFactory(_configuration).Create(attempt.Dbms);

        var schemaDescriptionFile = attempt.Problem.SchemaDescription.Files.FirstOrDefault(f => f.Dbms == attempt.Dbms);
        if (schemaDescriptionFile is null) throw new ApplicationException("Schema description file for expected solution does not exist");
        
        var (schemaDescription, solution) = await TaskEx.WhenAll(
            _fileService.ReadApplicationDirectoryFileAllTextAsync(schemaDescriptionFile.FilePath, cancellationToken),
            _fileService.ReadApplicationDirectoryFileAllTextAsync(attempt.SolutionPath, cancellationToken)
        );
        
        DbDataReader reader;
        string? error = null;
        await dbmsAdapter.GetLockAsync(cancellationToken);
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaAsync(schemaDescription, cancellationToken);
            reader = await dbmsAdapter.ExecuteQueryAsync(solution, cancellationToken);
        }
        catch (DbException e)
        {
            error = e.Message;
            reader = null!;
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }

        return (reader, error);
    }

    public async Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);
        if (attempt == null)
        {
            throw new ApplicationException("Attempt requested to be run does not exist");
        }
        
        DbDataReader expectedResult;
        DbDataReader solutionResult;
        string? error = null;
        try
        {
            (expectedResult, (solutionResult, error)) = await TaskEx.WhenAll(
                GetExpectedSolutionResult(attempt.ProblemId, cancellationToken),
                GetSolutionResult(attemptId, cancellationToken)
            );
        }
        catch (DbException)
        {
            return (AttemptStatus.SyntaxError, null);
        }
        
        if (error != null)
        {
            return (AttemptStatus.SyntaxError, error);
        }

        return ResultSetComparator.CompareRows(solutionResult, expectedResult, attempt.Problem.FloatMaxDelta,
                attempt.Problem.CaseSensitive, attempt.Problem.OrderMatters) switch
            {
                ResultSetComparisonResult.Equal => (AttemptStatus.Accepted, null),
                ResultSetComparisonResult.MismatchedRow => (AttemptStatus.WrongAnswer, null),
                ResultSetComparisonResult.MismatchedSchema => (AttemptStatus.WrongOutputFormat, null),
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}
