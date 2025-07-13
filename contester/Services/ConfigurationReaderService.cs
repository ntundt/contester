using System.Reflection;

namespace contester.Services;

public interface IConfigurationReaderService
{
    string GetApplicationDirectoryPath();
    int GetMaxUploadFileSizeBytes();
    string GetBackendUrl();
    string GetFrontendUrl();
    bool IsLoggingEnabled();
    TimeSpan GetSchemaCreationExecutionTimeout();
    TimeSpan GetSolutionExecutionTimeout();
}

public class ConfigurationReaderService(IConfiguration configuration) : IConfigurationReaderService
{
    public string GetApplicationDirectoryPath()
    {
        if (configuration["ApplicationDirectoryPath"] != null) return configuration["ApplicationDirectoryPath"]!;
        
        return GetDefaultApplicationDirectoryPath();
    }
    
    public int GetMaxUploadFileSizeBytes()
    {
        if (!int.TryParse(configuration["App:MaxUploadFileSizeBytes"], out var maxSize))
        {
            return Constants.DefaultMaxUploadFileSizeBytes;
        }
        return maxSize;
    }

    public string GetBackendUrl()
    {
        return configuration["App:BackendUrl"] ?? "https://localhost:7115";
    }

    public string GetFrontendUrl()
    {
        return configuration["App:FrontendUrl"] ?? "https://localhost:44497";
    }
    
    private static string GetDefaultApplicationDirectoryPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }

    public bool IsLoggingEnabled()
    {
        if (!bool.TryParse(configuration["App:LoggingEnabled"], out var loggingEnabled))
        {
            return false;
        }

        return loggingEnabled;
    }

    public TimeSpan GetSchemaCreationExecutionTimeout()
    {
        if (!int.TryParse(configuration["App:SchemaCreationExecutionTimeoutSeconds"], out var executionTimeoutSeconds))
        {
            return TimeSpan.FromSeconds(30);
        }
        
        return TimeSpan.FromSeconds(executionTimeoutSeconds);
    }

    public TimeSpan GetSolutionExecutionTimeout()
    {
        if (!int.TryParse(configuration["App:GetSolutionExecutionTimeout"], out var executionTimeoutSeconds))
        {
            return TimeSpan.FromSeconds(30);
        }
        
        return TimeSpan.FromSeconds(executionTimeoutSeconds);
    }

    public string GetSqlPlus()
    {
        return configuration["SqlPlusPath"] ?? "sqlplus";
    }

    public string? GetConnectionString(string name)
    {
        return configuration[$"ConnectionStrings:{name}"];
    }
}