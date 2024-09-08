using Microsoft.EntityFrameworkCore;

namespace diploma.Data.Init;

public class PostgresInitDbContext : DbContext, IInitDbContext
{
    private readonly ILogger<PostgresInitDbContext> _logger;
    public PostgresInitDbContext(DbContextOptions<PostgresInitDbContext> options, ILogger<PostgresInitDbContext> logger) : base(options)
    {
        _logger = logger;
    }

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
            _logger.LogWarning("Could not connect to PostgreSQL to init the database");
            return false;
        }
    }

    public void Init()
    {
        if (!InitNeeded())
        {
            return;
        }

        _logger.LogInformation("Initializing PostgreSQL database");

        try
        {
            Database.ExecuteSqlRaw(GetInitScript());
        }
        catch (Exception e)
        {
            _logger.LogWarning("Error initializing PostgreSQL database: {}", e.Message);
        }
    }
}