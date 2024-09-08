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

    private static string GetCheckInitNeededQuery()
    {
        return File.ReadAllText("Assets/Scripts/SqlServer/CheckInitNeeded.sql");
    }

    private static string GetInitScript()
    {
        return File.ReadAllText("Assets/Scripts/SqlServer/Init.sql");
    }

    private bool InitNeeded()
    {
        try
        {
            string query = GetCheckInitNeededQuery();
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
            Database.ExecuteSqlRaw(GetInitScript());
        } catch {
            // ignore
        }
    }
}