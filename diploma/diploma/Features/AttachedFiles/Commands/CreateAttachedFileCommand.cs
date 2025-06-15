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

public class CreateAttachedFileCommandHandler(
    IDirectoryService directoryService,
    ApplicationDbContext context,
    IConfigurationReaderService configuration)
    : IRequestHandler<CreateAttachedFileCommand, CreateAttachedFileCommandResult>
{
    private string GetFileUrl(Guid fileId)
    {
        return $"{configuration.GetBackendUrl()}/api/file/{fileId}";
    }

    private async Task SaveFile(IFormFile file, Guid fileId, CancellationToken cancellationToken)
    {
        var filePath = directoryService.GetAttachedFileFullPath(fileId);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);
    }

    public async Task<CreateAttachedFileCommandResult> Handle(CreateAttachedFileCommand request, CancellationToken cancellationToken)
    {
        if (request.File.Length > configuration.GetMaxUploadFileSizeBytes())
        {
            throw new NotifyUserException("File is too large");
        }

        var fileId = Guid.NewGuid();
        var filePath = directoryService.GetAttachedFileRelativePath(fileId);

        var attachedFile = new AttachedFile
        {
            Id = fileId,
            OriginalName = request.FileName ?? request.File.FileName,
            MimeType = request.File.ContentType,
            FilePath = filePath,
            AuthorId = request.CallerId,
        };

        await SaveFile(request.File, fileId, cancellationToken);

        context.AttachedFiles.Add(attachedFile);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateAttachedFileCommandResult
        {
            FileUrl = GetFileUrl(fileId),
        };
    }
}
