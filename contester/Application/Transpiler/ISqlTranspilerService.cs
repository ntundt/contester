namespace contester.Application.Transpiler;

public interface ISqlTranspilerService
{
    Task<string> TranspileAsync(string sql, string sourceDbms, string targetDbms, CancellationToken cancellationToken = default);
}
