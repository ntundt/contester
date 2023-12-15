using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using diploma.Application;
using diploma.Application.Extensions;
using diploma.Data;
using diploma.Features.Attempts;
using Microsoft.EntityFrameworkCore;

namespace diploma.Services;

public interface ISolutionRunnerService
{
    Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken);   
}

public class SolutionRunnerService : ISolutionRunnerService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    
    public SolutionRunnerService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private async Task<DbDataReader> GetExpectedSolutionResult(Guid problemId, CancellationToken cancellationToken)
    {
        var problem = await _context.Problems.AsNoTracking()
            .Include(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(p => p.Id == problemId, cancellationToken);
        if (problem is null) throw new ApplicationException("Problem with the GUID specified not found");

        var dbmsAdapter = new DbmsAdapterFactory(_configuration).Create(problem.SolutionDbms);

        var schemaDescriptionFile = problem.SchemaDescription.Files.FirstOrDefault(f => f.Dbms == problem.SolutionDbms);
        if (schemaDescriptionFile is null) throw new ApplicationException("Schema description file for expected solution does not exist");
        
        var (schemaDescription, solution) = await TaskEx.WhenAll(
            File.ReadAllTextAsync(schemaDescriptionFile.FilePath, cancellationToken),
            File.ReadAllTextAsync(problem.SolutionPath, cancellationToken)
        );
        
        DbDataReader reader;
        await dbmsAdapter.GetLockAsync(cancellationToken);
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaAsync(schemaDescription, cancellationToken);
            reader = await dbmsAdapter.ExecuteQueryAsync(solution, cancellationToken);
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }

        return reader;
    }
    
    private async Task<(DbDataReader, string?)> GetSolutionResult(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .ThenInclude(p => p.SchemaDescription)
            .ThenInclude(sd => sd.Files)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);
        if (attempt is null) throw new ApplicationException("Attempt with the GUID specified not found");

        var dbmsAdapter = new DbmsAdapterFactory(_configuration).Create(attempt.Dbms);

        var schemaDescriptionFile = attempt.Problem.SchemaDescription.Files.FirstOrDefault(f => f.Dbms == attempt.Dbms);
        if (schemaDescriptionFile is null) throw new ApplicationException("Schema description file for expected solution does not exist");
        
        var (schemaDescription, solution) = await TaskEx.WhenAll(
            File.ReadAllTextAsync(schemaDescriptionFile.FilePath, cancellationToken),
            File.ReadAllTextAsync(attempt.SolutionPath, cancellationToken)
        );
        
        DbDataReader reader;
        string? error = null;
        await dbmsAdapter.GetLockAsync(cancellationToken);
        try
        {
            await dbmsAdapter.DropCurrentSchemaAsync(cancellationToken);
            await dbmsAdapter.CreateSchemaAsync(schemaDescription, cancellationToken);
            reader = await dbmsAdapter.ExecuteQueryAsync(solution, cancellationToken);
        }
        catch (DbException e)
        {
            error = e.Message;
            reader = null!;
        }
        finally
        {
            dbmsAdapter.ReleaseLock();
        }

        return (reader, error);
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) || type == typeof(decimal);
    }

    private static decimal GetNumericAsDecimal(IDataRecord record, int index)
    {
        var type = record.GetFieldType(index);
        if (type == typeof(byte)) return record.GetByte(index);
        if (type == typeof(short)) return record.GetInt16(index);
        if (type == typeof(int)) return record.GetInt32(index);
        if (type == typeof(long)) return record.GetInt64(index);
        if (type == typeof(float)) return (decimal) record.GetFloat(index);
        if (type == typeof(double)) return (decimal) record.GetDouble(index);
        if (type == typeof(decimal)) return record.GetDecimal(index);
        throw new ApplicationException("Type is not numeric");
    }
    
    private static bool TypesCompatible(Type a, Type b)
    {
        if (a == b) return true;
        if (IsNumericType(a) && IsNumericType(b)) return true;
        return false;
    }
    
    private static bool CompareSchema(DbDataReader a, DbDataReader b)
    {
        var schemaA = a.GetColumnSchema();
        var schemaB = b.GetColumnSchema();

        return schemaA.All(column =>
                   schemaB.Any(bc => string.Equals(bc.ColumnName, column.ColumnName, StringComparison.CurrentCultureIgnoreCase)
                                     && TypesCompatible(bc.DataType!, column.DataType!)))
               && schemaB.All(column =>
                   schemaA.Any(ac => string.Equals(ac.ColumnName, column.ColumnName, StringComparison.CurrentCultureIgnoreCase)
                                     && TypesCompatible(ac.DataType!, column.DataType!)));
    }

    private static bool CompareRecords(IDataRecord a, IDataRecord b, decimal floatPrecision, bool caseSensitive)
    {
        for (var i = 0; i < a.FieldCount; i++)
        {
            var aType = a.GetFieldType(i);

            if (a.IsDBNull(i) && b.IsDBNull(i))
            {
                continue;
            }
            else if (IsNumericType(aType))
            {
                var aFloat = GetNumericAsDecimal(a, i);
                var bFloat = GetNumericAsDecimal(b, i);
                if (Math.Abs(aFloat - bFloat) > floatPrecision) return false;
            }
            else if (aType == typeof(string))
            {
                var aString = a.GetString(i).Trim();
                var bString = b.GetString(i).Trim();
                if (caseSensitive)
                {
                    if (aString != bString) return false;
                }
                else
                {
                    if (!string.Equals(aString, bString, StringComparison.CurrentCultureIgnoreCase)) return false;
                }
            }
            else
            {
                var aObject = a.GetValue(i);
                var bObject = b.GetValue(i);
                if (!aObject.Equals(bObject)) return false;
            }
        }

        return true;
    }

    private static bool CompareLinesUnordered(DbDataReader a, DbDataReader b, decimal floatPrecision, bool caseSensitive)
    {
        return a.Cast<IDataRecord>().All(aRecord => b.Cast<IDataRecord>().Any(bRecord => CompareRecords(aRecord, bRecord, floatPrecision, caseSensitive)))
            && b.Cast<IDataRecord>().All(bRecord => a.Cast<IDataRecord>().Any(aRecord => CompareRecords(aRecord, bRecord, floatPrecision, caseSensitive)));
    }

    private static bool CompareLinesOrdered(DbDataReader a, DbDataReader b, decimal floatPrecision, bool caseSensitive)
    {
        return a.Cast<IDataRecord>().Zip(b.Cast<IDataRecord>(), (aRecord, bRecord) => CompareRecords(aRecord, bRecord, floatPrecision, caseSensitive)).All(bb => bb)
            && a.Cast<IDataRecord>().Count() == b.Cast<IDataRecord>().Count();
    }
    
    public async Task<(AttemptStatus, string?)> RunAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts.AsNoTracking()
            .Include(a => a.Problem)
            .FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);
        if (attempt == null)
        {
            throw new ApplicationException("Attempt requested to be run does not exist");
        }
        
        DbDataReader expectedResult;
        DbDataReader solutionResult;
        string? error = null;
        try
        {
            (expectedResult, (solutionResult, error)) = await TaskEx.WhenAll(
                GetExpectedSolutionResult(attempt.ProblemId, cancellationToken),
                GetSolutionResult(attemptId, cancellationToken)
            );
        }
        catch (DbException)
        {
            return (AttemptStatus.SyntaxError, null);
        }
        
        if (error != null)
        {
            return (AttemptStatus.SyntaxError, error);
        }
        
        if (!CompareSchema(solutionResult, expectedResult))
        {
            return (AttemptStatus.WrongOutputFormat, null);
        }

        if (attempt.Problem.OrderMatters)
        {
            return CompareLinesOrdered(solutionResult, expectedResult, attempt.Problem.FloatMaxDelta,
                attempt.Problem.CaseSensitive)
                ? (AttemptStatus.Accepted, null)
                : (AttemptStatus.WrongAnswer, null);
        }

        return CompareLinesUnordered(solutionResult, expectedResult, attempt.Problem.FloatMaxDelta,
            attempt.Problem.CaseSensitive)
            ? (AttemptStatus.Accepted, null)
            : (AttemptStatus.WrongAnswer, null);
    }
}