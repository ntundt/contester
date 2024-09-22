using System.Data.Common;
using System.Data.SqlClient;
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

public class UpdateSchemaDescriptionFileCommandHandler : IRequestHandler<UpdateSchemaDescriptionFileCommand, SchemaDescriptionFileDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _configuration;
    
    public UpdateSchemaDescriptionFileCommandHandler(ApplicationDbContext context, IFileService fileService,
        IMapper mapper, IPermissionService permissionService, IConfiguration configuration)
    {
        _context = context;
        _fileService = fileService;
        _mapper = mapper;
        _permissionService = permissionService;
        _configuration = configuration;
    }
    
    public async Task<SchemaDescriptionFileDto> Handle(UpdateSchemaDescriptionFileCommand request, CancellationToken cancellationToken)
    {
        if (!await _permissionService.UserHasPermissionAsync(request.CallerId, Constants.Permission.ManageSchemaDescriptions, cancellationToken))
        {
            throw new UserDoesNotHavePermissionException(request.CallerId, Constants.Permission.ManageSchemaDescriptions);
        }
        
        var schemaDescriptionFile = await _context.SchemaDescriptionFiles
            .FirstOrDefaultAsync(s => s.SchemaDescriptionId == request.SchemaDescriptionId && s.Dbms == request.Dbms, cancellationToken);
        if (schemaDescriptionFile == null)
        {
            throw new SchemaDescriptionFileNotFoundException();
        }
        
        bool hasProblems = false;
        string problems = null!;
        
        var dbmsAdapter = new DbmsAdapterFactory(_configuration).Create(request.Dbms!);
        try
        {
            await dbmsAdapter.GetLockAsync(cancellationToken);
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaAsync(request.Description, cancellationToken);
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
        await _context.SaveChangesAsync(cancellationToken);

        await _fileService.SaveSchemaDescriptionToFileAsync(schemaDescriptionFile.SchemaDescriptionId, request.Dbms!, request.Description, cancellationToken);
        
        return _mapper.Map<SchemaDescriptionFileDto>(schemaDescriptionFile);
    }
}