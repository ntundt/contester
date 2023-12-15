using System.ComponentModel.DataAnnotations;

namespace diploma.Features.SchemaDescriptions;

/**
 * Schema description is either a SQL script that creates the schema or JSON file that describes the schema.
 * It is used to prepare the database for the solution execution.
 * Actual SQL scripts are stored on the disk.
 * @see Docs/Directories.md
 */
public class SchemaDescription
{
    public Guid Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = null!;
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
