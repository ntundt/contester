using System.Data.Common;

namespace contester.Infrastructure.Databases;

public class SqlServerAdapter(Func<DbConnection> connectionFactory) : DbmsAdapter(connectionFactory)
{
    private static async Task<string> GetDropCurrentSchemaSqlAsync()
    {
        return await File.ReadAllTextAsync("Assets/Scripts/MSSQLServer/DropSchema.sql");
    }
    
    public override async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = await GetDropCurrentSchemaSqlAsync();
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
