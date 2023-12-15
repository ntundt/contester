using System.Reflection;
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
}

public class DirectoryService : IDirectoryService
{
    private readonly IConfiguration _configuration;
    
    private bool ApplicationDirectoryPathIsSet()
    {
        return _configuration["ApplicationDirectoryPath"] != null;
    }
    
    private static string GetDefaultApplicationDirectoryPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }
    
    private string GetApplicationDirectoryPath()
    {
        if (_configuration["ApplicationDirectoryPath"] != null) return _configuration["ApplicationDirectoryPath"]!;
        
        return GetDefaultApplicationDirectoryPath();
    }
    
    private void EnsureApplicationDirectoryCreated()
    {
        var directoryPath = GetApplicationDirectoryPath();
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
    
    public DirectoryService(IConfiguration configuration, ILogger<DirectoryService> logger)
    {
        _configuration = configuration;
        EnsureApplicationDirectoryCreated();
        if (!ApplicationDirectoryPathIsSet())
        {
            logger.LogWarning("ApplicationDirectoryPath is not specified in appsettings.json. Using default path: {DefaultPath}", GetDefaultApplicationDirectoryPath());
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
        var directoryPath = Path.Combine(GetApplicationDirectoryPath(), directoryName);
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
}