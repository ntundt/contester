using diploma.Data;
using diploma.Exceptions;
using diploma.Services;
using MediatR;

namespace diploma.Features.AttachedFiles.Commands;

public class CreateAttachedFileCommand : IRequest<CreateAttachedFileCommandResult>
{
    public IFormFile File { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public Guid CallerId { get; set; }
}

public class CreateAttachedFileCommandResult
{
    public string FileUrl { get; set; } = null!;
}

public class CreateAttachedFileCommandHandler : IRequestHandler<CreateAttachedFileCommand, CreateAttachedFileCommandResult>
{
    private readonly IDirectoryService _directoryService;
    private readonly ApplicationDbContext _context;
    private readonly IConfigurationReaderService _configuration;

    public CreateAttachedFileCommandHandler(IDirectoryService directoryService, ApplicationDbContext context,
        IConfigurationReaderService configuration)
    {
        _directoryService = directoryService;
        _context = context;
        _configuration = configuration;
    }

    private string GetFileUrl(Guid fileId)
    {
        return $"{_configuration.GetAppUrl()}/api/file/{fileId}";
    }

    private async Task SaveFile(IFormFile file, Guid fileId, CancellationToken cancellationToken)
    {
        var filePath = _directoryService.GetAttachedFilePath(fileId);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);
    }

    public async Task<CreateAttachedFileCommandResult> Handle(CreateAttachedFileCommand request, CancellationToken cancellationToken)
    {
        if (request.File.Length > _configuration.GetMaxUploadFileSizeBytes())
        {
            throw new NotifyUserException("File is too large");
        }

        var fileId = Guid.NewGuid();
        var filePath = _directoryService.GetAttachedFilePath(fileId);

        var attachedFile = new AttachedFile
        {
            Id = fileId,
            OriginalName = request.FileName ?? request.File.FileName,
            MimeType = request.File.ContentType,
            FilePath = filePath,
            AuthorId = request.CallerId,
        };
        
        await SaveFile(request.File, fileId, cancellationToken);

        _context.AttachedFiles.Add(attachedFile);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateAttachedFileCommandResult
        {
            FileUrl = GetFileUrl(fileId),
        };
    }
}
