using System.Data.Common;
using System.Data.SqlClient;

namespace diploma.Application;

public interface IDbmsAdapter : IDisposable, IAsyncDisposable
{
    Task CreateSchemaTimeoutAsync(string description, TimeSpan timeout, CancellationToken cancellationToken);
    Task DropCurrentSchemaAsync(CancellationToken cancellationToken);
    Task<(DbConnection, DbCommand, DbDataReader)> ExecuteQueryTimeoutAsync(string query, TimeSpan timeout, CancellationToken cancellationToken);
    Task GetLockAsync(int priority);
    void ReleaseLock();
}
