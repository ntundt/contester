using System.Data;
using System.Data.Common;

namespace diploma.Services;

public enum ResultSetComparisonResult
{
    Equal,
    MismatchedSchema,
    MismatchedRow,
}

public static class ResultSetComparator
{
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

        if (schemaA.Count != schemaB.Count) return false;
        
        return schemaA.Zip(schemaB, (aColumn, bColumn) =>
            string.Equals(aColumn.ColumnName, bColumn.ColumnName, StringComparison.CurrentCultureIgnoreCase)
            && TypesCompatible(aColumn.DataType!, bColumn.DataType!)
        ).All(bb => bb);
    }

    private static bool CompareRecords(IDataRecord a, IDataRecord b, decimal floatPrecision, bool caseSensitive)
    {
        for (var i = 0; i < a.FieldCount; i++)
        {
            var aType = a.GetFieldType(i);

            if (a.IsDBNull(i) && !b.IsDBNull(i) || !a.IsDBNull(i) && b.IsDBNull(i))
            {
                return false;
            }

            if (a.IsDBNull(i) && b.IsDBNull(i))
            {
                continue;
            }
            
            if (IsNumericType(aType))
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

    public static ResultSetComparisonResult CompareRows(DbDataReader a, DbDataReader b, decimal floatPrecision, bool caseSensitive, bool orderMatters)
    {
        if (!CompareSchema(a, b))
        {
            return ResultSetComparisonResult.MismatchedSchema;
        }
        
        var aList = a.Cast<IDataRecord>().ToList();
        var bList = b.Cast<IDataRecord>().ToList();
        
        if (aList.Count != bList.Count) return ResultSetComparisonResult.MismatchedRow;

        if (orderMatters)
        {
            return CompareRowsOrdered(aList, bList, floatPrecision, caseSensitive) ?
                ResultSetComparisonResult.Equal : ResultSetComparisonResult.MismatchedRow;
        }
        
        return CompareRowsUnordered(aList, bList, floatPrecision, caseSensitive) ?
            ResultSetComparisonResult.Equal : ResultSetComparisonResult.MismatchedRow;
    }

    private static bool CompareRowsUnordered(List<IDataRecord> a, List<IDataRecord> b, decimal floatPrecision, bool caseSensitive)
    {
        return a.All(aRecord => b.Any(bRecord => CompareRecords(aRecord, bRecord, floatPrecision, caseSensitive)))
               && b.All(bRecord => a.Any(aRecord => CompareRecords(aRecord, bRecord, floatPrecision, caseSensitive)));
    }

    private static bool CompareRowsOrdered(List<IDataRecord> a, List<IDataRecord> b, decimal floatPrecision, bool caseSensitive)
    {
        return a.Zip(b, (aRecord, bRecord) => CompareRecords(aRecord, bRecord, floatPrecision, caseSensitive)).All(bb => bb);
    }
}
