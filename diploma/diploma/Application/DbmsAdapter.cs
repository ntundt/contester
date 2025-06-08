using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;

namespace diploma.Application;

public class PriorityMutex
{
    private class Waiter
    {
        public TaskCompletionSource<bool> Tcs { get; } = new TaskCompletionSource<bool>();
        public int Priority { get; }

        public Waiter(int priority)
        {
            Priority = priority;
        }
    }

    private readonly object _lock = new object();
    private readonly PriorityQueue<Waiter, int> _waitQueue = new();
    private bool _isLocked = false;

    public Task AcquireAsync(int priority)
    {
        lock (_lock)
        {
            if (!_isLocked && _waitQueue.Count == 0)
            {
                _isLocked = true;
                return Task.CompletedTask;
            }

            var waiter = new Waiter(priority);
            _waitQueue.Enqueue(waiter, -priority);
            return waiter.Tcs.Task;
        }
    }

    public void Release()
    {
        lock (_lock)
        {
            if (_waitQueue.Count > 0)
            {
                var next = _waitQueue.Dequeue();
                next.Tcs.SetResult(true);
            }
            else
            {
                _isLocked = false;
            }
        }
    }
}

public abstract class DbmsAdapter : IDbmsAdapter
{
    protected readonly Func<DbConnection> _connectionFactory;

    protected DbmsAdapter(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public virtual async Task CreateSchemaAsync(string description, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = description;
        command.CommandTimeout = 30;
        await command.ExecuteNonQueryAsync(cancellationToken);
        await connection.CloseAsync();
    }

    public virtual async Task CreateSchemaTimeoutAsync(string description, TimeSpan timeout,
        CancellationToken cancellationToken)
    { 
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var delayTask = Task.Delay(timeout, timeoutCts.Token);
        var commandExecutionTask = CreateSchemaAsync(description, timeoutCts.Token);
        if (await Task.WhenAny(commandExecutionTask, delayTask) == commandExecutionTask)
        {
            await timeoutCts.CancelAsync();
            await commandExecutionTask;
        }
        else
        {
            throw new TimeoutException();
        }
    }
    
    public abstract Task DropCurrentSchemaAsync(CancellationToken cancellationToken);

    protected virtual async Task<(DbConnection, DbCommand, DbDataReader)> ExecuteQueryAsync(string query, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = (int)timeout.TotalSeconds;
        var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
        return (connection, command, reader);
    }

    public virtual async Task<(DbConnection, DbCommand, DbDataReader)> ExecuteQueryTimeoutAsync(string query, TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var delayTask = Task.Delay(timeout, timeoutCts.Token);
        var queryExecutionTask = ExecuteQueryAsync(query, timeout, timeoutCts.Token);
        if (await Task.WhenAny(queryExecutionTask, delayTask) == queryExecutionTask)
        {
            await timeoutCts.CancelAsync();
            return await queryExecutionTask;
        }
        throw new TimeoutException();
    }
    
    private static readonly ConcurrentDictionary<string, PriorityMutex> Mutexes = new();

    public async Task GetLockAsync(int priority)
    {
        await using var connection = _connectionFactory();
        var key = connection.ConnectionString;
        var mutex = Mutexes.GetOrAdd(key, _ => new PriorityMutex());
        await mutex.AcquireAsync(priority);
        await connection.CloseAsync();
    }
    
    public void ReleaseLock()
    {
        using var connection = _connectionFactory();
        var key = connection.ConnectionString;
        var mutex = Mutexes.GetOrAdd(key, _ => new PriorityMutex());
        connection.Close();
        mutex.Release();
    }
    
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}