using System.Data.Common;

namespace contester.Features.Attempts.Services;

public class SolutionExecutionResult(
    DbConnection? connection,
    DbCommand? command,
    DbDataReader? reader,
    bool timeoutHit,
    string? error = null)
    : IAsyncDisposable
{
    public DbDataReader? Reader { get; set; } = reader;
    public bool TimeoutHit { get; set; } = timeoutHit;
    public string? Error { get; set; } = error;

    public async ValueTask DisposeAsync()
    {
        /*
         * Note: in PostgreSQL (at least), if there are multiple statements in a command, and the second or later
         * one of them contains a syntax error, it will throw upon attempt to close the connection. To mitigate
         * this, there are "ignore any DbException" try-catch blocks around every statement. This makes sure all
         * the resources are freed properly.
         */
        if (connection is not null)
            try
            {
                await connection.CloseAsync();
                await connection.DisposeAsync();
            } catch (DbException) { }

        if (command is not null)
            try
            {
                await command.DisposeAsync();
            } catch (DbException) { }

        if (Reader is not null)
            try
            {
                await Reader.CloseAsync();
                await Reader.DisposeAsync();
            } catch (DbException) { }
    }
}