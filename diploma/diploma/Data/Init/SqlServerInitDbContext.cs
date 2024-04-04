using Microsoft.EntityFrameworkCore;

namespace diploma.Data.Init;

public class SqlServerInitDbContext : DbContext, IInitDbContext
{
    private readonly ILogger<SqlServerInitDbContext> _logger;
    public SqlServerInitDbContext(DbContextOptions<SqlServerInitDbContext> options,
        ILogger<SqlServerInitDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    private bool InitNeeded()
    {
        try
        {
            string query = $"SELECT COUNT(*) AS Value FROM sys.databases WHERE name = 'SQL_CONTEST'";
            var aff = Database.SqlQueryRaw<int>(query).First();
            return aff == 0;
        } catch {
            _logger.LogWarning("Could not connect to SQL Server to init the database");
            return false;
        }
    }

    public void Init()
    {
        if (!InitNeeded())
        {
            return;
        }

        _logger.LogInformation("Initializing SQL Server database");

        try {
            Database.ExecuteSqlRaw("CREATE DATABASE SQL_CONTEST;");
            Database.ExecuteSqlRaw("USE SQL_CONTEST;");
            Database.ExecuteSqlRaw("CREATE LOGIN SQL_CONTEST_USER WITH PASSWORD = 'Password123';");
            Database.ExecuteSqlRaw("CREATE USER SQL_CONTEST_USER FOR LOGIN SQL_CONTEST_USER;");
            Database.ExecuteSqlRaw("GRANT CREATE TABLE TO SQL_CONTEST_USER;");
        } catch {
            // ignore
        }
    }
}