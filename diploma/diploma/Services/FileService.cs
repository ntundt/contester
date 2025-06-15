namespace diploma.Services;

public interface IFileService
{
    Task SaveAttemptToFileAsync(Guid attemptId, string attempt, CancellationToken cancellationToken);
    Task SaveProblemSolutionToFileAsync(Guid problemId, string dbms, string solution, CancellationToken cancellationToken);
    Task SaveProblemStatementToFileAsync(Guid problemId, string requestStatement, CancellationToken cancellationToken);
    Task SaveSchemaDescriptionToFileAsync(Guid schemaDescriptionId, string dbms, string schemaDescription, CancellationToken cancellationToken);
    Task SaveContestDescriptionToFileAsync(Guid contestId, string description, CancellationToken cancellationToken);
    Task<string> ReadApplicationDirectoryFileAllTextAsync(string filePath, CancellationToken cancellationToken);
    string ReadApplicationDirectoryFileAllText(string filePath);
}

public class FileService(IConfiguration configuration, IDirectoryService directoryService)
    : IFileService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SaveAttemptToFileAsync(Guid attemptId, string attempt, CancellationToken cancellationToken)
    {
        var filename = directoryService.GetAttemptFullPath(attemptId);
        await File.WriteAllTextAsync(filename, attempt, cancellationToken);
    }

    public async Task SaveProblemSolutionToFileAsync(Guid problemId, string dbms, string solution, CancellationToken cancellationToken)
    {
        var filename = directoryService.GetProblemSolutionFullPath(problemId, dbms);
        await File.WriteAllTextAsync(filename, solution, cancellationToken);
    }

    public async Task SaveProblemStatementToFileAsync(Guid problemId, string requestStatement, CancellationToken cancellationToken)
    {
        var filename = directoryService.GetProblemStatementFullPath(problemId);
        await File.WriteAllTextAsync(filename, requestStatement, cancellationToken);
    }

    public async Task SaveSchemaDescriptionToFileAsync(Guid schemaDescriptionId, string dbms, string schemaDescription, CancellationToken cancellationToken)
    {
        var filename = directoryService.GetSchemaDescriptionFullPath(schemaDescriptionId, dbms);
        await File.WriteAllTextAsync(filename, schemaDescription, cancellationToken);
    }

    public async Task SaveContestDescriptionToFileAsync(Guid contestId, string description, CancellationToken cancellationToken)
    {
        var filename = directoryService.GetContestDescriptionFullPath(contestId);
        await File.WriteAllTextAsync(filename, description, cancellationToken);
    }

    private string PrependPathWithApplicationDirectoryIfNeeded(string filePath)
    {
        if (Path.IsPathRooted(filePath))
        {
            return filePath;
        }
        return directoryService.PrependApplicationDirectoryPath(filePath);
    }

    public async Task<string> ReadApplicationDirectoryFileAllTextAsync(string filePath, CancellationToken cancellationToken)
    {
        filePath = PrependPathWithApplicationDirectoryIfNeeded(filePath);
        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }

    public string ReadApplicationDirectoryFileAllText(string filePath)
    {
        filePath = PrependPathWithApplicationDirectoryIfNeeded(filePath);
        return File.ReadAllText(filePath);
    }
}
