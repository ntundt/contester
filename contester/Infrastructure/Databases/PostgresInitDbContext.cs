using Microsoft.EntityFrameworkCore;

namespace contester.Infrastructure.Databases;

public class PostgresInitDbContext(
    DbContextOptions<PostgresInitDbContext> options,
    ILogger<PostgresInitDbContext> logger)
    : DbContext(options), IInitDbContext
{
    private static string GetCheckInitNeededQuery()
    {
        return File.ReadAllText("Assets/Scripts/PostgreSQL/CheckInitNeeded.sql");
    }

    private static string GetInitScript()
    {
        return File.ReadAllText("Assets/Scripts/PostgreSQL/Init.sql");
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
            logger.LogWarning("Could not connect to PostgreSQL to init the database");
            return false;
        }
    }

    public void Init()
    {
        if (!InitNeeded())
        {
            return;
        }

        logger.LogInformation("Initializing PostgreSQL database");

        try
        {
            Database.ExecuteSqlRaw(GetInitScript());
        }
        catch (Exception e)
        {
            logger.LogWarning("Error initializing PostgreSQL database: {}", e.Message);
        }
    }
}