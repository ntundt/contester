using Microsoft.EntityFrameworkCore;

namespace contester.Infrastructure.Databases;

public class OracleInitDbContext(DbContextOptions<OracleInitDbContext> options, ILogger<OracleInitDbContext> logger)
    : DbContext(options), IInitDbContext
{
    private static string GetCheckInitNeededQuery()
    {
        return File.ReadAllText("Assets/Scripts/Oracle/CheckInitNeeded.sql");
    }

    private static string GetInitScript()
    {
        return File.ReadAllText("Assets/Scripts/Oracle/Init.sql");
    }

    private bool InitNeeded()
    {
        try
        {
            string query = GetCheckInitNeededQuery();
            return Database.SqlQueryRaw<int>(query).First() == 0;
        }
        catch
        {
            logger.LogWarning("Could not connect to Oracle to init the database");
            return false;
        }
    }

    public void Init()
    {
        if (!InitNeeded())
        {
            return;
        }

        logger.LogInformation("Initializing Oracle database");

        try {
            Database.ExecuteSqlRaw("CREATE USER SQL_CONTEST_USER IDENTIFIED BY \"Password123\"");
            Database.ExecuteSqlRaw("ALTER USER SQL_CONTEST_USER QUOTA 32M ON USERS");
            Database.ExecuteSqlRaw("GRANT DBA TO SQL_CONTEST_USER");
        } catch (Exception ex) {
            // ignore
            Console.WriteLine(ex.Message);
        }
    }
}
