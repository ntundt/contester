using System.Data.Common;

namespace diploma.Application;

public class SqlServerAdapter : DbmsAdapter
{
    public SqlServerAdapter(DbConnection connection) : base(connection)
    {
    }
    
    public override async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
        var sql = @"
        DECLARE @disableConstraintsSQL NVARCHAR(MAX) = ''

        SELECT @disableConstraintsSQL += 'ALTER TABLE ' + QUOTENAME(TABLE_NAME) + ' DROP CONSTRAINT ' + QUOTENAME(CONSTRAINT_NAME) + ';'
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_SCHEMA = SCHEMA_NAME()

        EXEC sp_executesql @disableConstraintsSQL

        DECLARE @tableName NVARCHAR(255)

        DECLARE dropTables CURSOR FOR
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = SCHEMA_NAME()

        OPEN dropTables

        FETCH NEXT FROM dropTables INTO @tableName

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @dropTableSQL NVARCHAR(MAX)
            SET @dropTableSQL = 'DROP TABLE ' + QUOTENAME(@tableName)
            EXEC sp_executesql @dropTableSQL

            FETCH NEXT FROM dropTables INTO @tableName
        END

        CLOSE dropTables
        DEALLOCATE dropTables
        ";
        
        var command = _connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}