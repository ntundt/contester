namespace contester.Common.Sieve;

public class PaginatedResult<T>
{
    public List<T>? Data { get; set; }
    public int TotalCount { get; set; }
}
