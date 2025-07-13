using System.Data.Common;
using System.Data.SqlClient;
using AutoMapper;
using contester.Application;
using contester.Application.Transpiler;
using contester.Data;
using contester.Exceptions;
using contester.Features.Authentication.Exceptions;
using contester.Features.Authentication.Services;
using contester.Features.SchemaDescriptions.Exceptions;
using contester.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace contester.Features.SchemaDescriptions.Commands;

public class CreateSchemaDescriptionFileCommand : IRequest<SchemaDescriptionFileDto>
{
    public Guid CallerId { get; set; }
    public Guid SchemaDescriptionId { get; set; }
    public string Dbms { get; set; } = null!;
    public string? Description { get; set; }
    public string? SourceDbms { get; set; }
}

public class CreateSchemaDescriptionFileCommandHandler(
    ApplicationDbContext context,
    IDirectoryService directoryService,
    IMapper mapper,
    IPermissionService permissionService,
    IConfiguration configuration,
    ISqlTranspilerService sqlTranspilerService,
    IFileService fileService,
    IConfigurationReaderService configurationReaderService)
    : IRequestHandler<CreateSchemaDescriptionFileCommand, SchemaDescriptionFileDto>
{
    private async Task<string> TranspileAsync(SchemaDescription sd, string sourceDbms, string targetDbms, CancellationToken cancellationToken)
    {
        var schemaDescriptionFile = sd.Files.FirstOrDefault(f => f.Dbms == sourceDbms);
        if (schemaDescriptionFile is null) throw new SchemaDescriptionFileNotFoundException();
        if (schemaDescriptionFile.HasProblems) throw new NotifyUserException("Source schema description for transpilation has problems");
        
        var sql = await fileService.ReadApplicationDirectoryFileAllTextAsync(schemaDescriptionFile.FilePath, cancellationToken);
        return await sqlTranspilerService.TranspileAsync(sql, sourceDbms, targetDbms, cancellationToken);
    }

    public async Task<SchemaDescriptionFileDto> Handle(CreateSchemaDescriptionFileCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageSchemaDescriptions);
        }

        if (request.Description is null && request.SourceDbms is null)
        {
            throw new NotifyUserException("Either description or source dbms must be provided");
        }
        
        var schemaDescription = await context.SchemaDescriptions.AsNoTracking()
            .Include(s => s.Files)
            .FirstOrDefaultAsync(s => s.Id == request.SchemaDescriptionId, cancellationToken);
        if (schemaDescription is null) throw new SchemaDescriptionNotFoundException();
        
        var description = request.Description
            ?? await TranspileAsync(schemaDescription, request.SourceDbms!, request.Dbms, cancellationToken);

        bool hasProblems = false;
        string problems = null!;
        
        var dbmsAdapter = new DbmsAdapterFactory(configuration).CreateRandom(request.Dbms);
        try
        {
            await dbmsAdapter.GetLockAsync(3);
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaTimeoutAsync(description, 
                configurationReaderService.GetSchemaCreationExecutionTimeout(), cancellationToken);
        }
        catch (DbException e)
        {
            hasProblems = true;
            problems = e.InnerException?.Message ?? e.Message;
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }
        
        await fileService.SaveSchemaDescriptionToFileAsync(schemaDescription.Id, request.Dbms, description, cancellationToken);

        var schemaDescriptionFile = new SchemaDescriptionFile
        {
            Id = Guid.NewGuid(),
            FilePath = directoryService.GetSchemaDescriptionRelativePath(schemaDescription.Id, request.Dbms),
            Dbms = request.Dbms,
            SchemaDescriptionId = schemaDescription.Id,
            HasProblems = hasProblems,
            Problems = problems,
        };
        
        await context.SchemaDescriptionFiles.AddAsync(schemaDescriptionFile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<SchemaDescriptionFileDto>(schemaDescriptionFile);
    }
}
