using System.Data.Common;
using System.Globalization;
using diploma.Features.Attempts;

namespace diploma.Services;

public static class ResultSetEvaluator
{
    private static string GetColumnAsString(DbDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal)) return "NULL";
        var type = reader.GetFieldType(ordinal);
        
        if (type == typeof(byte)) return reader.GetByte(ordinal).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(short)) return reader.GetInt16(ordinal).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(int)) return reader.GetInt32(ordinal).ToString();
        if (type == typeof(long)) return reader.GetInt64(ordinal).ToString();
        if (type == typeof(float)) return reader.GetFloat(ordinal).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(double)) return reader.GetDouble(ordinal).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(decimal)) return reader.GetDecimal(ordinal).ToString(CultureInfo.InvariantCulture);
        if (type == typeof(string)) return reader.GetString(ordinal);
        if (type == typeof(DateTime)) return reader.GetDateTime(ordinal).ToUniversalTime().ToString(CultureInfo.InvariantCulture);
        if (type == typeof(Guid)) return reader.GetGuid(ordinal).ToString();
        if (type == typeof(bool)) return reader.GetBoolean(ordinal).ToString(CultureInfo.InvariantCulture);

        return "<Could not get the value as string>";
    }
    
    private static Row GetRow(DbDataReader reader)
    {
        var row = new Row();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            row.Columns.Add(GetColumnAsString(reader, i));
        }

        return row;
    }
    
    public static ResultSet EvaluateResultSet(DbDataReader reader)
    {
        var resultSet = new ResultSet
        {
            Columns = reader.GetColumnSchema().AsEnumerable().Select(x => new Column
            {
                Name = x.ColumnName,
                DataType = x.DataType!.Name,
            }).ToList()
        };

        while (reader.Read())
        {
            resultSet.Rows.Add(GetRow(reader));
        }
        
        return resultSet;
    }
}
