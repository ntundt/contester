namespace diploma.Features.Attempts;

public class Column
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
}

public class Schema
{
    public List<Column> Columns { get; set; } = new();
}

public class Row
{
    public List<string> Columns { get; set; } = new List<string>();
}

public class ResultSet
{
    public List<Column> Columns { get; set; } = new();
    public List<Row> Rows { get; set; } = new List<Row>();
}
