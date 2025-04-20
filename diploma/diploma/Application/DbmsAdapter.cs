using System.Collections.Concurrent;
using System.Data.Common;

namespace diploma.Application;

public abstract class DbmsAdapter : IDbmsAdapter
{
    protected readonly DbConnection _connection;

    protected DbmsAdapter(DbConnection connection)
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

    public virtual async Task CreateSchemaTimeoutAsync(string description, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource();
        var commandExecutionTask = CreateSchemaAsync(description, timeoutCancellationTokenSource.Token);
        if (await Task.WhenAny(commandExecutionTask, Task.Delay(timeout, cancellationToken)) == commandExecutionTask)
        {
            await commandExecutionTask;
            return;
        }
        timeoutCancellationTokenSource.CancelAsync();
        throw new TimeoutException();
    }
    
    public abstract Task DropCurrentSchemaAsync(CancellationToken cancellationToken);
    
    public virtual async Task<DbDataReader> ExecuteQueryAsync(string query, CancellationToken cancellationToken)
    {
        var command = _connection.CreateCommand();
        command.CommandText = query;
        var reader = await command.ExecuteReaderAsync(cancellationToken);
        return reader;
    }

    public virtual async Task<DbDataReader> ExecuteQueryTimeoutAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource();
        var queryExecutionTask = ExecuteQueryAsync(query, timeoutCancellationTokenSource.Token);
        if (await Task.WhenAny(queryExecutionTask, Task.Delay(timeout, cancellationToken)) == queryExecutionTask)
        {
            return await queryExecutionTask;
        }
        timeoutCancellationTokenSource.CancelAsync();
        throw new TimeoutException();
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