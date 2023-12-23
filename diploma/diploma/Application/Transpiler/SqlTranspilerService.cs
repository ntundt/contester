using System.Diagnostics;

namespace diploma.Application.Transpiler;

public class SqlTranspilerService : ISqlTranspilerService
{
    private readonly string _pythonPath;

    public SqlTranspilerService(IConfiguration configuration)
    {
        _pythonPath = configuration["PythonPath"] ?? "python";
    }
    
    private string ConvertDbmsName(string dbms)
    {
        return dbms switch
        {
            "PostgreSQL" => "postgresql",
            "MySQL" => "mysql",
            "SqlServer" => "tsql",
            "Oracle" => "oracle",
            _ => throw new Exception($"Unknown dbms {dbms}")
        };
    }
    
    public async Task<string> TranspileAsync(string sql, string sourceDbms, string targetDbms,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo(_pythonPath, $"./Assets/transpile.py --sourceDialect {ConvertDbmsName(sourceDbms)} --targetDialect {ConvertDbmsName(targetDbms)}")
        {
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception("Failed to start python process");
        }

        await process.StandardInput.WriteLineAsync(sql);
        await process.StandardInput.FlushAsync(cancellationToken);
        process.StandardInput.Close();
        
        var errors = await process.StandardError.ReadToEndAsync(cancellationToken);
        Console.WriteLine(errors);
        
        var result = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return result;
    }

}
