using contester.Features.ApplicationSettings;

namespace contester.Application;

public class ConnectionStringsCache
{
    private static ConnectionStringsCache? _instance;
    public static ConnectionStringsCache Instance => _instance ??= new ConnectionStringsCache();
    
    private List<ConnectionString> _connectionStrings = new List<ConnectionString>();
    
    private Random _random = new((int)(DateTime.UtcNow.Ticks / UInt32.MaxValue));

    public string GetRandomConnectionString(string dbms)
    {
        var filtered = _connectionStrings.Where(cs => cs.Dbms == dbms).ToList();
        return filtered[_random.Next(filtered.Count())].Text;
    }
    
    public void SetCachedValues(List<ConnectionString> connectionStrings) => _connectionStrings = connectionStrings;
}
