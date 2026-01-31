using System.Reflection;

namespace contester.Services;

public class ConfigurationParameterNotSetException(string parameterName)
    : ApplicationException($"Missing configuration parameter {parameterName}");

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
    string GetEmailUser();
    string GetEmailPassword();
    string GetEmailHost();
    int GetEmailPort();
    bool GetEmailUseStartTls();
    bool GetEmailDoNotCheckCertificateRevocation();
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

    public string GetEmailUser()
    {
        if (configuration["Email:User"] is not null)
        {
            return configuration["Email:User"]!;
        }

        throw new ConfigurationParameterNotSetException("Email:User");
    }

    public string GetEmailPassword()
    {
        if (configuration["Email:Password"] is not null)
        {
            return configuration["Email:Password"]!;
        }
        
        throw new ConfigurationParameterNotSetException("Email:Password");
    }

    public string GetEmailHost()
    {
        if (configuration["Email:Host"] is not null)
        {
            return configuration["Email:Host"]!;
        }
        
        throw new ConfigurationParameterNotSetException("Email:Host");
    }

    public int GetEmailPort()
    {
        if (configuration["Email:Port"] is not null && int.TryParse(configuration["Email:Port"], out var port))
        {
            return port;
        }

        return 465;
    }

    public bool GetEmailUseStartTls()
    {
        return configuration["Email:UseStartTls"] is not null &&
               bool.TryParse(configuration["Email:UseStartTls"], out var useStartTls) && useStartTls;
    }

    public bool GetEmailDoNotCheckCertificateRevocation()
    {
        return configuration["Email:DoNotCheckCertificateRevocation"] is not null &&
               bool.TryParse(configuration["Email:DoNotCheckCertificateRevocation"], out var trustCertificate) &&
               trustCertificate;
    }
}