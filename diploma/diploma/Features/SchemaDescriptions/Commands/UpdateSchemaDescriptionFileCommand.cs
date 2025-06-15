using System.Data.Common;
using AutoMapper;
using diploma.Application;
using diploma.Data;
using diploma.Features.Authentication.Exceptions;
using diploma.Features.Authentication.Services;
using diploma.Features.SchemaDescriptions.Exceptions;
using diploma.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diploma.Features.SchemaDescriptions.Commands;

public class UpdateSchemaDescriptionFileCommand : IRequest<SchemaDescriptionFileDto>
{
    public Guid CallerId { get; set; }
    public Guid SchemaDescriptionId { get; set; }
    public string? Dbms { get; set; }
    public string Description { get; set; } = null!;
}

public class UpdateSchemaDescriptionFileCommandHandler(
    ApplicationDbContext context,
    IFileService fileService,
    IMapper mapper,
    IPermissionService permissionService,
    IConfiguration configuration,
    IConfigurationReaderService configurationReaderService)
    : IRequestHandler<UpdateSchemaDescriptionFileCommand, SchemaDescriptionFileDto>
{
    public async Task<SchemaDescriptionFileDto> Handle(UpdateSchemaDescriptionFileCommand request, CancellationToken cancellationToken)
    {
        if (!await permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageSchemaDescriptions);
        }
        
        var schemaDescriptionFile = await context.SchemaDescriptionFiles
            .FirstOrDefaultAsync(s => s.SchemaDescriptionId == request.SchemaDescriptionId && s.Dbms == request.Dbms, cancellationToken);
        if (schemaDescriptionFile == null)
        {
            throw new SchemaDescriptionFileNotFoundException();
        }
        
        bool hasProblems = false;
        string problems = null!;
        
        var dbmsAdapter = new DbmsAdapterFactory(configuration).CreateRandom(request.Dbms!);
        await dbmsAdapter.GetLockAsync(3);
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaTimeoutAsync(request.Description,
                configurationReaderService.GetSchemaCreationExecutionTimeout(), cancellationToken);
        }
        catch (DbException e)
        {
            hasProblems = true;
            problems = e.Message;
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }
        
        schemaDescriptionFile.HasProblems = hasProblems;
        schemaDescriptionFile.Problems = problems;
        await context.SaveChangesAsync(cancellationToken);

        await fileService.SaveSchemaDescriptionToFileAsync(schemaDescriptionFile.SchemaDescriptionId, request.Dbms!, request.Description, cancellationToken);
        
        return mapper.Map<SchemaDescriptionFileDto>(schemaDescriptionFile);
    }
}
