using System.Reflection;
using diploma.Features.AttachedFiles;
using diploma.Features.Attempts;
using diploma.Features.Contests;
using diploma.Features.Problems;
using diploma.Features.SchemaDescriptions;

namespace diploma.Services;

public interface IDirectoryService
{
    string GetDirectory(string directoryName);
    string GetContestDescriptionPath(Guid contestId);
    Task SaveContestDescriptionToFileAsync(Guid contestId, string description, CancellationToken cancellationToken);
    string GetSchemaDescriptionPath(Guid schemaDescriptionId, string dbms);
    Task SaveSchemaDescriptionToFileAsync(Guid schemaDescriptionId, string dbms, string schemaDescription, CancellationToken cancellationToken);
    string GetProblemStatementPath(Guid problemId);
    Task SaveProblemStatementToFileAsync(Guid problemId, string requestStatement, CancellationToken cancellationToken);
    string GetProblemSolutionPath(Guid problemId, string dbms);
    Task SaveProblemSolutionToFileAsync(Guid problemId, string dbms, string solution, CancellationToken cancellationToken);
    string GetAttemptPath(Guid attemptId);
    Task SaveAttemptToFileAsync(Guid attemptId, string attempt, CancellationToken cancellationToken);
    string GetAttachedFilePath(Guid fileId);
    string PrependApplicationDirectoryPath(string path);
}

public class DirectoryService : IDirectoryService
{
    private readonly IConfigurationReaderService _configuration;
    
    private static string GetDefaultApplicationDirectoryPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }
    
    private void EnsureApplicationDirectoryCreated()
    {
        var directoryPath = _configuration.GetApplicationDirectoryPath();
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to create application directory at {directoryPath}", e);
        }
    }
    
    public DirectoryService(IConfigurationReaderService configuration, ILogger<DirectoryService> logger)
    {
        _configuration = configuration;
        EnsureApplicationDirectoryCreated();
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
        return directoryPath;
    }
    
    public string GetContestDescriptionPath(Guid contestId)
    {
        return Path.Combine(GetDirectory(nameof(Contest)), $"{contestId}-description.md");
    }

    public async Task SaveContestDescriptionToFileAsync(Guid contestId, string description, CancellationToken cancellationToken)
    {
        var filename = GetContestDescriptionPath(contestId);
        await File.WriteAllTextAsync(filename, description, cancellationToken);
    }
    
    public string GetSchemaDescriptionPath(Guid schemaDescriptionId, string dbms)
    {
        return Path.Combine(GetDirectory(nameof(SchemaDescription)), $"{schemaDescriptionId}-{dbms}.sql");
    }
    
    public async Task SaveSchemaDescriptionToFileAsync(Guid schemaDescriptionId, string dbms, string schemaDescription, CancellationToken cancellationToken)
    {
        var filename = GetSchemaDescriptionPath(schemaDescriptionId, dbms);
        await File.WriteAllTextAsync(filename, schemaDescription, cancellationToken);
    }
    
    public string GetProblemStatementPath(Guid problemId)
    {
        return Path.Combine(GetDirectory(nameof(Problem)), $"{problemId}-statement.md");
    }
    
    public async Task SaveProblemStatementToFileAsync(Guid problemId, string requestStatement, CancellationToken cancellationToken)
    {
        var filename = GetProblemStatementPath(problemId);
        await File.WriteAllTextAsync(filename, requestStatement, cancellationToken);
    }
    
    public string GetProblemSolutionPath(Guid problemId, string dbms)
    {
        return Path.Combine(GetDirectory(nameof(Problem)), $"{problemId}-solution-{dbms}.sql");
    }
    
    public async Task SaveProblemSolutionToFileAsync(Guid problemId, string dbms, string solution, CancellationToken cancellationToken)
    {
        var filename = GetProblemSolutionPath(problemId, dbms);
        await File.WriteAllTextAsync(filename, solution, cancellationToken);
    }
    
    public string GetAttemptPath(Guid attemptId)
    {
        return Path.Combine(GetDirectory(nameof(Attempt)), $"{attemptId}.sql");
    }
    
    public async Task SaveAttemptToFileAsync(Guid attemptId, string attempt, CancellationToken cancellationToken)
    {
        var filename = GetAttemptPath(attemptId);
        await File.WriteAllTextAsync(filename, attempt, cancellationToken);
    }

    public string GetAttachedFilePath(Guid fileId)
    {
        return Path.Combine(GetDirectory(nameof(AttachedFile)), $"{fileId}.bin");
    }

    public string PrependApplicationDirectoryPath(string path)
    {
        return Path.Combine(_configuration.GetApplicationDirectoryPath(), path);
    }
}