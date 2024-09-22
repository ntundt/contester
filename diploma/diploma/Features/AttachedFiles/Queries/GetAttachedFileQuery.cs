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

public class GetAttachedFileQueryHandler : IRequestHandler<GetAttachedFileQuery, GetAttachedFileQueryResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IDirectoryService _directoryService;

    public GetAttachedFileQueryHandler(IDirectoryService directoryService, ApplicationDbContext context)
    {
        _context = context;
        _directoryService = directoryService;
    }

    public async Task<GetAttachedFileQueryResult> Handle(GetAttachedFileQuery request, CancellationToken cancellationToken)
    {
        var attachedFile = await _context.AttachedFiles.FindAsync(request.FileId);
        if (attachedFile == null)
        {
            throw new NotifyUserException("Attached file not found");
        }

        Stream fileStream;
        try
        {
            fileStream = new FileStream(_directoryService.PrependApplicationDirectoryPath(attachedFile.FilePath), FileMode.Open);
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
