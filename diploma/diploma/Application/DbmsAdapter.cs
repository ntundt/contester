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

public abstract class DbmsAdapter(Func<DbConnection> connectionFactory) : IDbmsAdapter
{
    protected readonly Func<DbConnection> _connectionFactory = connectionFactory;

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

    protected virtual string GetVerifyDbmsAvailableCommandText()
    {
        return "select 1";
    }

    public async Task<(bool, string?)> VerifyDbmsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = _connectionFactory();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var delayTask = Task.Delay(TimeSpan.FromSeconds(30), timeoutCts.Token);
            using var connectTask = connection.OpenAsync(cancellationToken);
            if (await Task.WhenAny(connectTask, delayTask) == delayTask)
            {
                await timeoutCts.CancelAsync();
                return (false, "Connection timed out");
            }

            await timeoutCts.CancelAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = GetVerifyDbmsAvailableCommandText();
            command.CommandTimeout = 30;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return (false, "Could not read a row");
            if (await reader.GetFieldValueAsync<decimal>(0, cancellationToken) != 1)
                return (false, "Could not read a field value");
            
            await connection.CloseAsync();
            await reader.CloseAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
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
        try
        {
            return await ExecuteQueryAsync(query, timeout, timeoutCts.Token);
        }
        catch (DbException ex)
        {
            if (ex.InnerException is TimeoutException)
                throw ex.InnerException;
            throw;
        }
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