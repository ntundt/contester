using System.Data.Common;
using System.Data.SqlClient;
using AutoMapper;
using contester.Application;
using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.Problems.Exceptions;
using contester.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.Problems.Commands;

public class UpdateProblemCommand : IRequest<ProblemDto>
{
    public Guid CallerId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Statement { get; set; } = null!;
    public bool OrderMatters { get; set; }
    public decimal FloatMaxDelta { get; set; }
    public bool CaseSensitive { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public int MaxGrade { get; set; }
    public int Ordinal { get; set; }
    public Guid SchemaDescriptionId { get; set; }
    public string Solution { get; set; } = null!;
    public string SolutionDbms { get; set; } = null!;
}

public class UpdateProblemCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IPermissionService permissionService,
    IConfiguration configuration,
    IFileService fileService,
    IConfigurationReaderService configurationReaderService)
    : IRequestHandler<UpdateProblemCommand, ProblemDto>
{
    public async Task<ProblemDto> Handle(UpdateProblemCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageProblems, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageProblems);
        }
        
        var problem = await context.Problems
            .Include(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (problem == null)
        {
            throw new ProblemNotFoundException();
        }
        problem.Name = request.Name;
        problem.OrderMatters = request.OrderMatters;
        problem.FloatMaxDelta = request.FloatMaxDelta;
        problem.CaseSensitive = request.CaseSensitive;
        problem.TimeLimit = request.TimeLimit;
        problem.MaxGrade = request.MaxGrade;
        problem.SchemaDescriptionId = request.SchemaDescriptionId;
        problem.SolutionDbms = request.SolutionDbms;
        problem.StatementPath = directoryService.GetProblemStatementRelativePath(problem.Id);
        problem.SolutionPath = directoryService.GetProblemSolutionRelativePath(problem.Id, request.SolutionDbms);

        var targetSchemaDescription = await context.SchemaDescriptions.AsNoTracking()
            .Include(sd => sd.Files)
            .FirstOrDefaultAsync(sd => sd.Id == request.SchemaDescriptionId, cancellationToken);
        if (targetSchemaDescription is null)
        {
            throw new NotifyUserException("Schema description specified not found");
        }

        var schemaFile = 
            targetSchemaDescription.Files.FirstOrDefault(f => f.Dbms == request.SolutionDbms) 
                ?? throw new NotifyUserException("Schema description for DBMS specified not found");
        var schema = await fileService.ReadApplicationDirectoryFileAllTextAsync(schemaFile.FilePath, cancellationToken);
        
        var dbmsAdapter = new DbmsAdapterFactory(configuration).CreateRandom(request.SolutionDbms);

        await dbmsAdapter.GetLockAsync(3);
        DbConnection? connection = null;
        DbCommand? command = null;
        DbDataReader? reader = null;
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaTimeoutAsync(schema,
                configurationReaderService.GetSchemaCreationExecutionTimeout(), cancellationToken);
            (connection, command, reader) = await dbmsAdapter.ExecuteQueryTimeoutAsync(request.Solution,
                configurationReaderService.GetSolutionExecutionTimeout(), cancellationToken);
        }
        catch (DbException e)
        {
            throw new ProblemSolutionInvalidException(e.Message);
        }
        finally
        {
            /*
             * Refer to SolutionCheckerService.cs for explanation.
             */
            if (connection is not null)
                try {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                } catch (DbException) { }
            if (command is not null)
                try
                {
                    await command.DisposeAsync();
                } catch (DbException) { }
            if (reader is not null)
                try {
                    await reader.CloseAsync();
                    await reader.DisposeAsync();
                } catch (DbException) { }
            
            dbmsAdapter.ReleaseLock();
        }
        
        await fileService.SaveProblemStatementToFileAsync(problem.Id, request.Statement, cancellationToken);
        await fileService.SaveProblemSolutionToFileAsync(problem.Id, request.SolutionDbms, request.Solution, cancellationToken);

        var oldOrdinal = problem.Ordinal;
        var newOrdinal = request.Ordinal;
        var problems = await context.Problems
            .Where(p => p.ContestId == problem.ContestId && p.Id != problem.Id)
            .OrderBy(p => p.Ordinal)
            .ToListAsync(cancellationToken);
        foreach (var p in problems)
        {
            if (oldOrdinal < newOrdinal)
            {
                if (p.Ordinal > oldOrdinal && p.Ordinal <= newOrdinal)
                {
                    p.Ordinal--;
                }
            }
            else
            {
                if (p.Ordinal < oldOrdinal && p.Ordinal >= newOrdinal)
                {
                    p.Ordinal++;
                }
            }
        }
        problem.Ordinal = request.Ordinal;

        await context.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<ProblemDto>(problem);
    }
}
