using System.Data;
using System.Data.Common;
using diploma.Services;

namespace diploma.Application;

public class OracleAdapter : DbmsAdapter
{
    private string _sqlplus;
    private string _connectionString;
    public OracleAdapter(DbConnection connection, string sqlplus, string connectionString) : base(connection)
    {
        _sqlplus = sqlplus;
        _connectionString = connectionString;
    }
    
    public override async Task CreateSchemaAsync(string description, CancellationToken cancellationToken)
    {
        /*var commands = description.Split(";").Select(cmd => cmd.Trim()).Where(cmd => !string.IsNullOrEmpty(cmd));
        foreach (var cmd in commands)
        {
            var command = _connection.CreateCommand();
            command.CommandText = cmd;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }*/
        
        var sqlPlusService = new SqlPlusService(_sqlplus, _connectionString);
        await sqlPlusService.ExecuteScript(description, cancellationToken);
    }

    private static async Task<string> GetDropCurrentSchemaSqlAsync()
    {
        return await File.ReadAllTextAsync("Assets/Scripts/Oracle/DropSchema.sql");
    }

    public override async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
        var command = _connection.CreateCommand();
        command.CommandText = await GetDropCurrentSchemaSqlAsync();
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
