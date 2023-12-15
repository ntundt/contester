using System.Data.Common;
using System.Data.SqlClient;

namespace diploma.Application;

public interface IDbmsAdapter : IDisposable
{
    Task<bool> IsDbmsAvailableAsync(CancellationToken cancellationToken);
    Task CreateSchemaAsync(string description, CancellationToken cancellationToken);
    Task DropCurrentSchemaAsync(CancellationToken cancellationToken);
    Task<DbDataReader> ExecuteQueryAsync(string query, CancellationToken cancellationToken);
    Task GetLockAsync(CancellationToken cancellationToken);
    void ReleaseLock();
}