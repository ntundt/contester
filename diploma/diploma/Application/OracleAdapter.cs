using System.Data;
using System.Data.Common;

namespace diploma.Application;

public class OracleAdapter : DbmsAdapter
{
    public OracleAdapter(DbConnection connection) : base(connection)
    {
    }
    
    public override async Task CreateSchemaAsync(string description, CancellationToken cancellationToken)
    {
        var commands = description.Split(";").Select(cmd => cmd.Trim()).Where(cmd => !string.IsNullOrEmpty(cmd));
        foreach (var cmd in commands)
        {
            var command = _connection.CreateCommand();
            command.CommandText = cmd;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
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
