using System.Data.Common;

namespace diploma.Application;

public class OracleAdapter : DbmsAdapter
{
    public OracleAdapter(DbConnection connection) : base(connection)
    {
    }
    
    public override async Task CreateSchemaAsync(string description, CancellationToken cancellationToken)
    {
        // split by both ;\n and ;\r\n
        var commands = description.Split(";\n").SelectMany(cmd => cmd.Split(";\r\n")).Select(cmd => cmd.Trim()).Where(cmd => !string.IsNullOrEmpty(cmd));
        foreach (var cmd in commands)
        {
            var command = _connection.CreateCommand();
            command.CommandText = cmd;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public override async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
        var sql = @"DECLARE
          tableName VARCHAR2(255);
          constraintName VARCHAR2(255);
        BEGIN
          FOR constraintRec IN (SELECT table_name, constraint_name
                               FROM user_constraints
                               WHERE constraint_type = 'R') LOOP
            EXECUTE IMMEDIATE 'ALTER TABLE ' || constraintRec.table_name || ' DISABLE CONSTRAINT ' || constraintRec.constraint_name;
          END LOOP;

          FOR tableRec IN (SELECT table_name FROM user_tables) LOOP
            EXECUTE IMMEDIATE 'DROP TABLE ""' || tableRec.table_name || '""';
          END LOOP;
        END;
";

        var command = _connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}