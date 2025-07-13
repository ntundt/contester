using System.ComponentModel.DataAnnotations;
using contester.Features.Contests;

namespace contester.Features.SchemaDescriptions;

/**
 * Schema description is a collection of SQL scripts that create schema and populate it with data.
 * It is used to prepare the database for the solution execution.
 * Actual SQL scripts are stored on the disk.
 */
public class SchemaDescription
{
    public Guid Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    public Guid? ContestId { get; set; }
    public Contest? Contest { get; set; } = null!;
    public List<SchemaDescriptionFile> Files { get; set; } = null!;
}

public class SchemaDescriptionFile
{
    public Guid Id { get; set; }
    [MaxLength(260)]
    public string FilePath { get; set; } = null!;
    [MaxLength(50)]
    public string Dbms { get; set; } = null!;
    public Guid SchemaDescriptionId { get; set; }
    public bool HasProblems { get; set; }
    [MaxLength(128)]
    public string? Problems { get; set; }
    public SchemaDescription SchemaDescription { get; set; } = null!;
}
