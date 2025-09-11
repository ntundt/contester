using System.Reflection;

namespace contester.Services;

public interface IConfigurationReaderService
{
    string GetApplicationDirectoryPath();
    int GetMaxUploadFileSizeBytes();
    string GetBackendUrl();
    string GetFrontendUrl();
    string GetAdminUserEmail();
    String GetAdminUserPassword();
    bool IsPasswordlessAuthenticationEnabled();
    bool IsLoggingEnabled();
    TimeSpan GetSchemaCreationExecutionTimeout();
    TimeSpan GetSolutionExecutionTimeout();
    string GetJwtKey();
    string GetJwtIssuer();
    TimeSpan GetAccessTokenTtl();
    TimeSpan GetRefreshTokenTtl();
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

    public string GetAdminUserEmail()
    {
        return configuration["App:AdminUserEmail"] ?? "admin@contest.er";
    }

    public string GetAdminUserPassword()
    {
        return configuration["App:AdminUserPassword"] ?? "admin";
    }

    public bool IsPasswordlessAuthenticationEnabled()
    {
        if (!bool.TryParse(configuration["App:UsePasswordlessAuthentication"], out var passwordlessAuthenticationEnabled))
        {
            return true;
        }

        return passwordlessAuthenticationEnabled;
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

    public string GetJwtKey()
    {
        if (configuration["Jwt:Key"] is not null)
        {
            return configuration["Jwt:Key"]!;
        }

        throw new ApplicationException("Jwt:Key is not set");
    }
    
    public string GetJwtIssuer()
    {
        if (configuration["Jwt:Issuer"] is not null)
        {
            return configuration["Jwt:Issuer"]!;
        }
        
        throw new ApplicationException("Jwt:Issuer is not set");
    }

    public TimeSpan GetAccessTokenTtl()
    {
        if (!int.TryParse(configuration["Jwt:AccessTokenTtlSeconds"], out var accessTokenTtlSeconds))
        {
            return TimeSpan.FromSeconds(600);
        }
        
        return TimeSpan.FromSeconds(accessTokenTtlSeconds);
    }
    
    public TimeSpan GetRefreshTokenTtl()
    {
        if (!int.TryParse(configuration["Jwt:RefreshTokenTtlSeconds"], out var refreshTokenTtlSeconds))
        {
            return TimeSpan.FromDays(2);
        }
        
        return TimeSpan.FromSeconds(refreshTokenTtlSeconds);
    }

}