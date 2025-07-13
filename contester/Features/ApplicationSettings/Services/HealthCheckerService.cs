using System.Diagnostics;
using contester.Application;

namespace contester.Features.ApplicationSettings.Services;

public class HealthCheckerService(IConfiguration configuration)
{
    public async Task<(bool, string?, TimeSpan)> HealthCheck(string connectionString, string dbms, CancellationToken cancellationToken)
    {
        IDbmsAdapter? dbmsAdapter = null;
        try
        {
            dbmsAdapter = new DbmsAdapterFactory(configuration)
                .CreateWithConnectionString(dbms, connectionString);
            await dbmsAdapter.GetLockAsync(3);
            var timer = Stopwatch.StartNew();
            var (healthy, message) = await dbmsAdapter.VerifyDbmsAvailableAsync(cancellationToken);
            timer.Stop();
            return (healthy, message, timer.Elapsed);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, TimeSpan.Zero);
        }
        finally
        {
            try
            {
                dbmsAdapter?.ReleaseLock();
            }
            catch (Exception)
            {
                // Probably something related to malformed connection string. Ignore.
            }
        }
    }
}
