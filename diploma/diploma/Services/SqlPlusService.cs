using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace diploma.Services;

public class OracleSqlPlusException : DbException
{
    public OracleSqlPlusException(string? message) : base(message) { }
}

public class SqlPlusService
{
    private readonly string _sqlplus;
    private readonly string _connectionString;

    private readonly Regex _sp2ErrorRegex = new Regex(".*SP2-\\d{4}.*");
    
    public SqlPlusService(string sqlplus, string connectionString)
    {
        _sqlplus = sqlplus;
        _connectionString = connectionString;
    }

    private DbConnectionStringBuilder GetConnectionStringBuilder()
    {
        var builder = new DbConnectionStringBuilder();
        builder.ConnectionString = _connectionString;
        return builder;
    }

    private string GetConnectionStringAttribute(string attributeName)
    {
        var builder = GetConnectionStringBuilder();
        var attribute = builder[attributeName] as string;
        if (attribute is null) throw new ApplicationException($"Could not get attribute {attributeName}");
        return attribute;
    }

    private string GetUsername()
    {
        return GetConnectionStringAttribute("User ID");
    }

    private string GetPassword()
    {
        return GetConnectionStringAttribute("Password");
    }

    private string GetDataSource()
    {
        return GetConnectionStringAttribute("Data Source");
    }

    public async Task ExecuteScript(string sql, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo(
                _sqlplus,
                $"{GetUsername()}/{GetPassword()}@{GetDataSource()}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
            };
            var process = Process.Start(startInfo);
            if (process == null) throw new ApplicationException("Could not start sqlplus process");

            var output = "";
            var error = "";
            
            process.OutputDataReceived += (sender, args) =>
                output += $"{args.Data?.ToString()}\n";
            process.ErrorDataReceived += (sender, args) =>
                error += args.Data?.ToString();
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.StandardInput.WriteLineAsync("WHENEVER SQLERROR EXIT SQL.SQLCODE");
            await process.StandardInput.WriteLineAsync("SET TERMOUT OFF");
            await process.StandardInput.WriteLineAsync(sql);
            await process.StandardInput.WriteLineAsync("EXIT");
            await process.StandardInput.FlushAsync(cancellationToken);
            process.StandardInput.Close();
            
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new OracleSqlPlusException($"ORA-{process.ExitCode.ToString().PadLeft(5, '0')}: {error}");
            }

            var sp2Errors = output.Split('\n').Where(x => _sp2ErrorRegex.Match(x).Success);
            if (sp2Errors.Any())
            {
                throw new OracleSqlPlusException($"{sp2Errors.First()}");
            }
        }
        catch (OracleSqlPlusException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Could not execute script: {sql}", ex);
        }
    }
}
