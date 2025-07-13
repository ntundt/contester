using diploma.Data;
using diploma.Exceptions;
using diploma.Services;
using MediatR;

namespace diploma.Features.AttachedFiles.Queries;

public class GetAttachedFileQuery : IRequest<GetAttachedFileQueryResult>
{
    public Guid FileId { get; set; }
}

public class GetAttachedFileQueryResult
{
    public Stream File { get; set; } = null!;
    public string FileName { get; set; } = null!;
}

public class GetAttachedFileQueryHandler(IDirectoryService directoryService, ApplicationDbContext context)
    : IRequestHandler<GetAttachedFileQuery, GetAttachedFileQueryResult>
{
    public async Task<GetAttachedFileQueryResult> Handle(GetAttachedFileQuery request, CancellationToken cancellationToken)
    {
        var attachedFile = await context.AttachedFiles.FindAsync(request.FileId, cancellationToken);
        if (attachedFile == null)
        {
            throw new NotifyUserException("Attached file not found");
        }

        Stream fileStream;
        try
        {
            fileStream = new FileStream(directoryService.PrependApplicationDirectoryPath(attachedFile.FilePath), FileMode.Open);
        }
        catch (Exception)
        {
            throw new NotifyUserException("Failed to open file");
        }

        return new GetAttachedFileQueryResult
        {
            File = fileStream,
            FileName = attachedFile.OriginalName
        };
    }
}
