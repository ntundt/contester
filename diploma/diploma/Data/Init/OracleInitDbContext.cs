using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace diploma.Data.Init;

public class OracleInitDbContext : DbContext, IInitDbContext
{
    private readonly ILogger<OracleInitDbContext> _logger;
    public OracleInitDbContext(DbContextOptions<OracleInitDbContext> options, ILogger<OracleInitDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    private bool InitNeeded()
    {
        try
        {
            string query = $"SELECT COUNT(*) AS \"Value\" FROM all_users \"t\" WHERE username = 'SQL_CONTEST_USER'";
            return Database.SqlQueryRaw<int>(query).First() == 0;
        }
        catch
        {
            _logger.LogWarning("Could not connect to Oracle to init the database");
            return false;
        }
    }

    public void Init()
    {
        if (!InitNeeded())
        {
            return;
        }

        _logger.LogInformation("Initializing Oracle database");

        try {
            Database.ExecuteSqlRaw("CREATE USER SQL_CONTEST_USER IDENTIFIED BY \"Password123\"");
            Database.ExecuteSqlRaw("ALTER USER SQL_CONTEST_USER QUOTA 32M ON USERS");
            Database.ExecuteSqlRaw("GRANT CREATE SESSION, CREATE TABLE TO SQL_CONTEST_USER");
        } catch {
            // ignore
        }
    }
}