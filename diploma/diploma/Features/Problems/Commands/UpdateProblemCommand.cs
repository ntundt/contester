﻿using System.Data.Common;
using System.Data.SqlClient;
using AutoMapper;
using diploma.Application;
using diploma.Data;
using diploma.Exceptions;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.Problems.Exceptions;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.Problems.Commands;

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

public class UpdateProblemCommandHandler : IRequestHandler<UpdateProblemCommand, ProblemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly IConfigurationReaderService _configurationReaderService;
    
    public UpdateProblemCommandHandler(ApplicationDbContext context, IDirectoryService directoryService, IMapper mapper,
        IPermissionService permissionService, IConfiguration configuration, IFileService fileService,
        IConfigurationReaderService configurationReaderService)
    {
        _context = context;
        _directoryService = directoryService;
        _mapper = mapper;
        _permissionService = permissionService;
        _configuration = configuration;
        _fileService = fileService;
        _configurationReaderService = configurationReaderService;
    }
    
    public async Task<ProblemDto> Handle(UpdateProblemCommand request, CancellationToken cancellationToken)
    {
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageProblems, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageProblems);
        }
        
        var problem = await _context.Problems
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
        problem.StatementPath = _directoryService.GetProblemStatementRelativePath(problem.Id);
        problem.SolutionPath = _directoryService.GetProblemSolutionRelativePath(problem.Id, request.SolutionDbms);

        var targetSchemaDescription = await _context.SchemaDescriptions.AsNoTracking()
            .Include(sd => sd.Files)
            .FirstOrDefaultAsync(sd => sd.Id == request.SchemaDescriptionId, cancellationToken);
        if (targetSchemaDescription is null)
        {
            throw new NotifyUserException("Schema description specified not found");
        }

        var schemaFile = 
            targetSchemaDescription.Files.FirstOrDefault(f => f.Dbms == request.SolutionDbms) 
                ?? throw new NotifyUserException("Schema description for DBMS specified not found");
        var schema = await _fileService.ReadApplicationDirectoryFileAllTextAsync(schemaFile.FilePath, cancellationToken);
        
        var dbmsAdapter = new DbmsAdapterFactory(_configuration).Create(request.SolutionDbms);

        await dbmsAdapter.GetLockAsync(cancellationToken);
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaTimeoutAsync(schema,
                _configurationReaderService.GetSchemaCreationExecutionTimeout(), cancellationToken);
            await dbmsAdapter.ExecuteQueryTimeoutAsync(request.Solution,
                _configurationReaderService.GetSolutionExecutionTimeout(), cancellationToken);
        }
        catch (DbException e)
        {
            throw new ProblemSolutionInvalidException(e.Message);
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }
        
        await _fileService.SaveProblemStatementToFileAsync(problem.Id, request.Statement, cancellationToken);
        await _fileService.SaveProblemSolutionToFileAsync(problem.Id, request.SolutionDbms, request.Solution, cancellationToken);

        var oldOrdinal = problem.Ordinal;
        var newOrdinal = request.Ordinal;
        var problems = await _context.Problems
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

        await _context.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<ProblemDto>(problem);
    }
}
