using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.SqlClient;

namespace diploma.Application;

public class DbmsAdapter : IDbmsAdapter
{
    protected readonly DbConnection _connection;
    
    public DbmsAdapter(DbConnection connection)
    {
        _connection = connection;
        _connection.Open();
    }
    
    public virtual async Task<bool> IsDbmsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _connection.OpenAsync(cancellationToken);
        } 
        catch (DbException)
        {
            return false;
        }
        finally
        {
            await _connection.CloseAsync();
        }
        return true;
    }
    
    public virtual async Task CreateSchemaAsync(string description, CancellationToken cancellationToken)
    {
        var command = _connection.CreateCommand();
        command.CommandText = description;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
    
    public virtual async Task DropCurrentSchemaAsync(CancellationToken cancellationToken)
    {
    }
    
    public async Task<DbDataReader> ExecuteQueryAsync(string query, CancellationToken cancellationToken)
    {
        var command = _connection.CreateCommand();
        command.CommandText = query;
        var reader = await command.ExecuteReaderAsync(cancellationToken);
        return reader;
    }
    
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Mutexes = new();

    public async Task GetLockAsync(CancellationToken cancellationToken)
    {
        var mutex = Mutexes.GetOrAdd(_connection.ConnectionString, _ => new SemaphoreSlim(1, 1));
        await mutex.WaitAsync(cancellationToken);
    }
    
    public void ReleaseLock()
    {
        var mutex = Mutexes.GetOrAdd(_connection.ConnectionString, _ => new SemaphoreSlim(1, 1));
        mutex.Release();
    }
    
    public void Dispose()
    {
        _connection.Dispose();
    }
}