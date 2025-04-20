using System.Data.Common;
using System.Data.SqlClient;

namespace diploma.Application;

public interface IDbmsAdapter : IDisposable
{
    Task<bool> IsDbmsAvailableAsync(CancellationToken cancellationToken);
    Task CreateSchemaTimeoutAsync(string description, TimeSpan timeout, CancellationToken cancellationToken);
    Task DropCurrentSchemaAsync(CancellationToken cancellationToken);
    Task<DbDataReader> ExecuteQueryTimeoutAsync(string query, TimeSpan timeout, CancellationToken cancellationToken);
    Task GetLockAsync(CancellationToken cancellationToken);
    void ReleaseLock();
}