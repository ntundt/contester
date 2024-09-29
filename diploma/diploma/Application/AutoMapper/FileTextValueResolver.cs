using AutoMapper;
using diploma.Services;

namespace diploma.Application.AutoMapper;

public class FileTextValueResolver : IMemberValueResolver<object, object, string, string>
{
    private readonly IFileService _fileService;

    public FileTextValueResolver(IFileService fileService)
    {
        _fileService = fileService;
    }

    public string Resolve(object source, object destination, string sourceMember, string destinationMember, ResolutionContext context)
    {
        try 
        {
            return _fileService.ReadApplicationDirectoryFileAllText(sourceMember);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading file: " + e.Message);
            return "<Could not read the file>";
        }
    }
}
