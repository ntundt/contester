using contester.Features.Authentication.Services;
using contester.Infrastructure.Seeders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace tests.integration;

public class ContesterWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dataConnectionString;
    private readonly string _applicationDirectoryPath;
    private readonly Mock<IAuthorizationService> _authorizationService = new();
    private readonly Mock<IAdminUserSeeder> _adminUserSeeder = new();

    public Guid CurrentUserId { get; set; }

    public ContesterWebApplicationFactory(string dataConnectionString, string applicationDirectoryPath)
    {
        _dataConnectionString = dataConnectionString;
        _applicationDirectoryPath = applicationDirectoryPath;

        _authorizationService.Setup(s => s.GetUserId()).Returns(() => CurrentUserId);
        _authorizationService.Setup(s => s.TryGetUserId()).Returns(() => CurrentUserId);
        _adminUserSeeder.Setup(s => s.Seed()).Returns(Task.CompletedTask);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dataConnectionString,
                ["ConnectionStrings:PostgresAdminConnection"] = _dataConnectionString,
                ["ConnectionStrings:SqlServerAdminConnection"] =
                    "Server=127.0.0.1,1433;Database=master;User Id=sa;Password=Password123;Encrypt=False",
                ["ConnectionStrings:OracleAdminConnection"] =
                    "Data Source=127.0.0.1:1521/xe;User Id=system;Password=oracle;",
                ["ApplicationDirectoryPath"] = _applicationDirectoryPath,
                ["App:BackendUrl"] = "http://localhost",
                ["App:FrontendUrl"] = "http://localhost",
                ["App:LoggingEnabled"] = "false",
                ["App:SchemaCreationExecutionTimeoutSeconds"] = "30",
                ["App:AdminUserEmail"] = "admin@contest.er",
                ["App:AdminUserPassword"] = "admin",
                ["App:UsePasswordlessAuthentication"] = "true",
                ["Jwt:Issuer"] = "https://localhost",
                ["Jwt:Key"] = "AVeryLongAndStrongKey-IntegrationTests-123456",
                ["Jwt:AccessTokenTtlSeconds"] = "600",
                ["Jwt:RefreshTokenTtlSeconds"] = "3600",
                ["Email:Host"] = "localhost",
                ["Email:Port"] = "25",
                ["Email:User"] = "test",
                ["Email:Password"] = "test",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAdminUserSeeder>();
            services.AddSingleton(_adminUserSeeder.Object);

            services.RemoveAll<IAuthorizationService>();
            services.AddSingleton(_authorizationService.Object);
        });
    }
}
