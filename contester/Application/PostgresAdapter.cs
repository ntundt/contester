using System.Data.Common;

namespace contester.Application;

public class PostgresAdapter : DbmsAdapter
{
    public PostgresAdapter(Func<DbConnection> connectionFactory) : base(connectionFactory)
    { }

    private static async Task<string> GetDropCurrentSchemaSqlAsync()
    {
        return await File.ReadAllTextAsync("Assets/Scripts/PostgreSQL/DropSchema.sql");
    }

    public override async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        
        await using var command = connection.CreateCommand();
        command.CommandText = await GetDropCurrentSchemaSqlAsync();
        await command.ExecuteNonQueryAsync(cancellationToken);
        await connection.CloseAsync();
    }
}
