using System.Reflection;

namespace diploma.Services;

public interface IConfigurationReaderService
{
    string GetApplicationDirectoryPath();
    int GetMaxUploadFileSizeBytes();
    string GetBackendUrl();
    string GetFrontendUrl();
    bool IsLoggingEnabled();
}

public class ConfigurationReaderService : IConfigurationReaderService
{
    private readonly IConfiguration _configuration;
    
    public ConfigurationReaderService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GetApplicationDirectoryPath()
    {
        if (_configuration["ApplicationDirectoryPath"] != null) return _configuration["ApplicationDirectoryPath"]!;
        
        return GetDefaultApplicationDirectoryPath();
    }
    
    public int GetMaxUploadFileSizeBytes()
    {
        if (!int.TryParse(_configuration["App:MaxUploadFileSizeBytes"], out var maxSize))
        {
            return Constants.DefaultMaxUploadFileSizeBytes;
        }
        return maxSize;
    }

    public string GetBackendUrl()
    {
        return _configuration["App:BackendUrl"] ?? "https://localhost:7115";
    }

    public string GetFrontendUrl()
    {
        return _configuration["App:FrontendUrl"] ?? "https://localhost:44497";
    }
    
    private static string GetDefaultApplicationDirectoryPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }

    public bool IsLoggingEnabled()
    {
        if (!bool.TryParse(_configuration["App:LoggingEnabled"], out var loggingEnabled))
        {
            return false;
        }

        return loggingEnabled;
    }
}