using Microsoft.EntityFrameworkCore;

namespace contester.Infrastructure.Databases;

public class SqlServerInitDbContext(
    DbContextOptions<SqlServerInitDbContext> options,
    ILogger<SqlServerInitDbContext> logger)
    : DbContext(options), IInitDbContext
{
    private static string GetCheckInitNeededQuery()
    {
        return File.ReadAllText("Assets/Scripts/MSSQLServer/CheckInitNeeded.sql");
    }

    private static string GetInitScript()
    {
        return File.ReadAllText("Assets/Scripts/MSSQLServer/Init.sql");
    }

    private bool InitNeeded()
    {
        try
        {
            string query = GetCheckInitNeededQuery();
            var aff = Database.SqlQueryRaw<int>(query).First();
            return aff == 0;
        } catch {
            logger.LogWarning("Could not connect to SQL Server to init the database");
            return false;
        }
    }

    public void Init()
    {
        if (!InitNeeded())
        {
            return;
        }

        logger.LogInformation("Initializing SQL Server database");

        try {
            Database.ExecuteSqlRaw(GetInitScript());
        } catch {
            // ignore
        }
    }
}