using System.Reflection;
using contester.Features.AttachedFiles;
using contester.Features.Attempts;
using contester.Features.Contests;
using contester.Features.Problems;
using contester.Features.SchemaDescriptions;

namespace contester.Services;

public interface IDirectoryService
{
    string GetDirectory(string directoryName);
    string GetContestDescriptionFullPath(Guid contestId);
    string GetContestDescriptionRelativePath(Guid contestId);
    string GetSchemaDescriptionFullPath(Guid schemaDescriptionId, string dbms);
    string GetSchemaDescriptionRelativePath(Guid schemaDescriptionId, string dbms);
    string GetProblemStatementFullPath(Guid problemId);
    string GetProblemStatementRelativePath(Guid problemId);
    string GetProblemSolutionFullPath(Guid problemId, string dbms);
    string GetProblemSolutionRelativePath(Guid problemId, string dbms);
    string GetAttemptFullPath(Guid attemptId);
    string GetAttemptRelativePath(Guid attemptId);
    string GetAttachedFileFullPath(Guid fileId);
    string GetAttachedFileRelativePath(Guid fileId);
    string PrependApplicationDirectoryPath(string path);
}

public class DirectoryService : IDirectoryService
{
    private readonly IConfigurationReaderService _configuration;
    
    private static string GetDefaultApplicationDirectoryPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }
    
    public DirectoryService(IConfigurationReaderService configuration, ILogger<DirectoryService> logger)
    {
        _configuration = configuration;
        EnsureApplicationDirectoryCreated();
    }

    private void EnsureApplicationDirectoryCreated()
    {
        var directoryPath = _configuration.GetApplicationDirectoryPath();
        EnsureDirectoryCreated(directoryPath);
    }

    private void EnsureDirectoryCreated(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to create directory at {directoryPath}", e);
        }
    }
    
    /// <summary>
    /// Gets subdirectory of the application directory. Creates it if it doesn't exist.
    /// </summary>
    /// <param name="directoryName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string GetDirectory(string directoryName)
    {
        var directoryPath = Path.Combine(_configuration.GetApplicationDirectoryPath(), directoryName);
        EnsureDirectoryCreated(directoryPath);
        return directoryPath;
    }

    public string GetRelativeDirectory(string directoryName)
    {
        var directoryPath = Path.Combine(_configuration.GetApplicationDirectoryPath(), directoryName);
        EnsureDirectoryCreated(directoryPath);
        return directoryName;
    }

    public string GetContestDescriptionFullPath(Guid contestId)
    {
        return PrependApplicationDirectoryPath(GetContestDescriptionRelativePath(contestId));
    }
    
    public string GetContestDescriptionRelativePath(Guid contestId)
    {
        return Path.Combine(GetRelativeDirectory(nameof(Contest)), $"{contestId}-description.md");
    }

    public string GetSchemaDescriptionFullPath(Guid schemaDescriptionId, string dbms)
    {
        return PrependApplicationDirectoryPath(GetSchemaDescriptionRelativePath(schemaDescriptionId, dbms));
    }
    
    public string GetSchemaDescriptionRelativePath(Guid schemaDescriptionId, string dbms)
    {
        return Path.Combine(GetRelativeDirectory(nameof(SchemaDescription)), $"{schemaDescriptionId}-{dbms}.sql");
    }
    
    public string GetProblemStatementFullPath(Guid problemId)
    {
        return PrependApplicationDirectoryPath(GetProblemStatementRelativePath(problemId));
    }

    public string GetProblemStatementRelativePath(Guid problemId)
    {
        return Path.Combine(GetRelativeDirectory(nameof(Problem)), $"{problemId}-statement.md");
    }
    
    public string GetProblemSolutionFullPath(Guid problemId, string dbms)
    {
        return PrependApplicationDirectoryPath(GetProblemSolutionRelativePath(problemId, dbms));
    }

    public string GetProblemSolutionRelativePath(Guid problemId, string dbms)
    {
        return Path.Combine(GetRelativeDirectory(nameof(Problem)), $"{problemId}-solution-{dbms}.sql");
    }
    
    public string GetAttemptFullPath(Guid attemptId)
    {
        return PrependApplicationDirectoryPath(GetAttemptRelativePath(attemptId));
    }

    public string GetAttemptRelativePath(Guid attemptId)
    {
        return Path.Combine(GetRelativeDirectory(nameof(Attempt)), $"{attemptId}.sql");
    }

    public string GetAttachedFileFullPath(Guid fileId)
    {
        return PrependApplicationDirectoryPath(GetAttachedFileRelativePath(fileId));
    }

    public string GetAttachedFileRelativePath(Guid fileId)
    {
        return Path.Combine(GetRelativeDirectory(nameof(AttachedFile)), $"{fileId}.bin");
    }

    public string PrependApplicationDirectoryPath(string path)
    {
        return Path.Combine(_configuration.GetApplicationDirectoryPath(), path);
    }
}