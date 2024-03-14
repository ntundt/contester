using System.Reflection;

namespace diploma.Services;

public interface IConfigurationReaderService
{
    string GetApplicationDirectoryPath();
    int GetMaxUploadFileSizeBytes();
    string GetAppUrl();
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

    public string GetAppUrl()
    {
        return _configuration["App:Url"] ?? "http://localhost:5000";
    }
    
    private static string GetDefaultApplicationDirectoryPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }
}