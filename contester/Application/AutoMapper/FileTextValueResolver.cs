using AutoMapper;
using contester.Services;

namespace contester.Application.AutoMapper;

public class FileTextValueResolver : IMemberValueResolver<object, object, string, string>
{
    private readonly IFileService _fileService;
    private readonly ILogger<FileTextValueResolver> _logger;

    public FileTextValueResolver(IFileService fileService, ILogger<FileTextValueResolver> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public string Resolve(object source, object destination, string sourceMember, string destinationMember, ResolutionContext context)
    {
        try 
        {
            return _fileService.ReadApplicationDirectoryFileAllText(sourceMember);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e.ToString());
            return "<Could not read the file>";
        }
    }
}
